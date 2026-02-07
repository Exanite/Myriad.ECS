using System;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// An archetype contains all entities which share exactly the same set of components.
/// </summary>
public sealed class Archetype
{
    /// <inheritdoc cref="EntityStorage"/>
    private EntityStorage storage;

    /// <inheritdoc cref="ArchetypeInfo"/>
    internal readonly ArchetypeInfo Info;

    /// <summary>
    /// The total number of entities in this archetype.
    /// </summary>
    private int entityCount;

    internal readonly int Id;

    /// <summary>
    /// The world which this archetype belongs to.
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Components => Info.Components;

    /// <summary>
    /// All entities in this archetype.
    /// </summary>
    public ReadOnlySpan<Entity> Entities => storage.EntityColumn.AsSpan(0, entityCount);

    /// <summary>
    /// The max number of entities that can be stored in this archetype without resizing.
    /// </summary>
    public int Capacity => storage.Capacity;

    internal Archetype(int id, EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        Id = id;
        World = world;
        Info = new ArchetypeInfo(components);
        storage = new EntityStorage(in Info, EcsConstants.ArchetypeInitialCapacity);
    }

    /// <summary>
    /// Ensures that the archetype has at least the specified capacity.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        if (storage.Capacity >= capacity)
        {
            return;
        }

        // Save old storage
        var oldStorage = storage;
        var oldRange = new EntityStorageRange(in oldStorage, 0, entityCount);

        // Reallocate storage
        capacity = M.GetNextPowerOfTwo(capacity);
        storage = new EntityStorage(in Info, capacity);

        // Copy from old to new
        var newRange = new EntityStorageRange(in storage, 0, entityCount);
        oldRange.CopyAllTo(newRange);
    }

    internal void AddEntity(EntityId entityId, ref EntityLocation location)
    {
        EnsureCapacity(entityCount + 1);

        // Use the next free slot
        var entityIndex = entityCount++;

        // Store the entity in this archetype
        storage.EntityColumn[entityIndex] = entityId.ToEntity(World);

        // Update the storage location to refer to this archetype
        location.IndexInArchetype = entityIndex;
        location.Archetype = this;
    }

    internal void RemoveEntity(EntityLocation location)
    {
        var currentEntityIndex = location.IndexInArchetype;
        var currentRange = new EntityStorageRange(in storage, currentEntityIndex, 1);

        // We are guaranteed to have at least 1 entity
        var lastEntityIndex = entityCount - 1;
        var lastRange = new EntityStorageRange(in storage, lastEntityIndex, 1);

        var isSameLocation = currentEntityIndex == lastEntityIndex;
        if (!isSameLocation)
        {
            // Update location
            var lastEntity = storage.EntityColumn[lastEntityIndex];
            ref var lastLocation = ref World.Entities.GetLocation(lastEntity.EntityId);
            lastLocation.IndexInArchetype = currentEntityIndex;

            // Swap last to current
            lastRange.CopyAllTo(currentRange);
        }

        // Clear last
        lastRange.Clear();

        // Update entity count
        entityCount--;
    }

    internal void MigrateEntity(EntityId entity, Archetype dstArchetype, ref EntityLocation location)
    {
        GuardUtility.IsFalse(dstArchetype == this, "Destination archetype is the same as the source archetype");

        // Copy current location so we can use it later
        var srcLocation = location;

        // Move the entity to the new archetype
        dstArchetype.AddEntity(entity, ref location);

        // Copy across everything that exists in the destination archetype
        for (var i = 0; i < storage.ComponentColumns.Length; i++)
        {
            var componentId = Info.ComponentIdByColumnIndex[i].Value;

            // Skip if the target archetype does not have this component
            if (componentId >= dstArchetype.Info.ColumnIndexByComponentId.Length || dstArchetype.Info.ColumnIndexByComponentId[componentId] == -1)
            {
                continue;
            }

            // Copy from source archetype to destination
            var srcArray = storage.ComponentColumns[i];
            var dstArray = dstArchetype.storage.ComponentColumns[dstArchetype.Info.ColumnIndexByComponentId[componentId]];
            Array.Copy(srcArray, srcLocation.IndexInArchetype, dstArray, location.IndexInArchetype, 1);
        }

        // Remove the entity from this archetype (using the old saved location)
        RemoveEntity(srcLocation);
    }

    /// <summary>
    /// Copies the component data from the source archetype as new entities.
    /// </summary>
    internal void AddFrom(Archetype srcArchetype, EntityLookup lookup)
    {
        EnsureCapacity(entityCount + srcArchetype.Entities.Length);

        // Copy component data
        var srcRange = new EntityStorageRange(in srcArchetype.storage, 0, srcArchetype.Entities.Length);
        var dstRange = new EntityStorageRange(in storage, entityCount, srcArchetype.Entities.Length);
        srcRange.CopyComponentsTo(dstRange);

        // Allocate new entity ids
        for (var i = 0; i < srcArchetype.Entities.Length; i++)
        {
            // Allocate an entity id and point it to this archetype
            ref var location = ref World.Entities.AcquireId(out var entityId);
            location.IndexInArchetype = dstRange.StartIndex + i;
            location.Archetype = this;

            // Store the entity in this archetype
            storage.EntityColumn[dstRange.StartIndex + i] = entityId.ToEntity(World);

            // Add the entity pair to the lookup dictionary
            var originalEntity = srcArchetype.Entities[i];
            var newEntity = entityId.ToEntity(World);
            lookup.Add(originalEntity, newEntity);
        }

        // Update entity count
        entityCount += srcArchetype.entityCount;
    }

    /// <summary>
    /// Destroy every entity in this archetype.
    /// </summary>
    internal void Clear()
    {
        var range = new EntityStorageRange(in storage, 0, entityCount);
        range.Clear();

        entityCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>() where T : IComponent
    {
        // Unsafe.As() is safe because we have to get the column index using the component id
        // If the component id is invalid, then we will get an index out of bounds
        var componentId = ComponentId.Get<T>();
        var array = GetComponentArray(componentId);
        return Unsafe.As<Array, T[]>(ref array).AsSpan(0, entityCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Array GetComponentArray(ComponentId id)
    {
        return storage.ComponentColumns[Info.ColumnIndexByComponentId[id.Value]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex) where T : IComponent
    {
        return ref GetSpan<T>()[entityIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Info.Hash.GetHashCode();
    }
}
