using System;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Allocations;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs.Worlds.Chunks;

public sealed class Chunk
{
    /// <summary>
    /// The archetype which contains this chunk.
    /// </summary>
    public Archetype Archetype { get; }

    /// <summary>
    /// Map from component index to component type.
    /// </summary>
    internal readonly Type[] ComponentTypesByComponentIndex;

    /// <summary>
    /// Map from component index to component ID.
    /// </summary>
    internal readonly ComponentId[] ComponentIdByComponentIndex;

    /// <summary>
    /// Sparse map from component ID to component index in chunk.
    /// </summary>
    internal readonly int[] ComponentIndexByComponentId;

    /// <summary>
    /// Sparse map from component ID to component event dispatcher for component types stored by this chunk.
    /// </summary>
    internal readonly ComponentEventDispatcher[] ComponentEventDispatcherByComponentId;

    /// <remarks>
    /// Indexed using entity index.
    /// </remarks>>
    private readonly Entity[] entities;

    /// <remarks>
    /// Indexed using component index, then entity index.
    /// </remarks>>
    private readonly Array[] components;

    /// <summary>
    /// Get the number of entities currently in this chunk.
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// All entities in this chunk.
    /// </summary>
    public ReadOnlySpan<Entity> Entities => entities.AsSpan(0, EntityCount);

    internal Chunk(Archetype archetype, int entityCapacity)
    {
        Archetype = archetype;

        ComponentTypesByComponentIndex = archetype.ComponentTypesByComponentIndex;
        ComponentIndexByComponentId = archetype.ComponentIndexByComponentId;
        ComponentIdByComponentIndex = archetype.ComponentIdByComponentIndex;
        ComponentEventDispatcherByComponentId = archetype.ComponentEventDispatcherByComponentId;

        entities = new Entity[entityCapacity];
        components = new Array[ComponentTypesByComponentIndex.Length];
        for (var i = 0; i < components.Length; i++)
        {
            components[i] = ArrayFactory.Create(ComponentTypesByComponentIndex[i], entityCapacity);
        }
    }

    #region Get component

    public Span<T> GetSpan<T>() where T : IComponent
    {
        return GetSpan<T>(ComponentId.Get<T>());
    }

    public Span<T> GetSpan<T>(ComponentId id) where T : IComponent
    {
        return GetComponentArray<T>(id).AsSpan(0, EntityCount);
    }

    internal T[] GetComponentArray<T>() where T : IComponent
    {
        return GetComponentArray<T>(ComponentId.Get<T>());
    }

    internal T[] GetComponentArray<T>(ComponentId id) where T : IComponent
    {
        return (T[])GetComponentArray(id);
    }

    internal Array GetComponentArray(ComponentId id)
    {
        return components[ComponentIndexByComponentId[id.Value]];
    }

    internal ref T Get<T>(EntityId entityId, int entityIndex) where T : IComponent
    {
        AssertUtility.IsTrue(entities[entityIndex].EntityId == entityId, "Mismatched entities in chunk");
        return ref Get<T>(entityIndex);
    }

    internal ValueRef<T> GetRef<T>(EntityId entityId, int entityIndex) where T : IComponent
    {
        AssertUtility.IsTrue(entities[entityIndex].EntityId == entityId, "Mismatched entities in chunk");

        return new ValueRef<T>(ref Get<T>(entityIndex));
    }

    internal ref T Get<T>(int entityIndex) where T : IComponent
    {
        return ref Get<T>(entityIndex, ComponentId.Get<T>());
    }

    internal ref T Get<T>(int entityIndex, ComponentId id) where T : IComponent
    {
        return ref GetSpan<T>(id)[entityIndex];
    }

    #endregion

    #region Add/remove entity

    // Note that these must be called only from Archetype! The Archetype needs to do some bookeeping on create/destroy.

    internal void Clear()
    {
        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var component in components)
        {
            Array.Clear(component, 0, component.Length);
        }

        // Not strictly necessary, clean up all the IDs so they're default instead of some invalid value.
        Array.Clear(entities, 0, entities.Length);

        EntityCount = 0;
    }

    internal EntityStorageLocation AddEntity(EntityId entity, ref StorageLocation location)
    {
        // It is safe to only assert here. It should never happen if Myriad is working
        // correctly. If it does somehow go wrong you'll get an index out of range exception
        // below so it still fails in a sensible way.
        AssertUtility.IsTrue(EntityCount < entities.Length, "Cannot add entity to full chunk");

        // Use the next free slot
        var entityIndex = EntityCount++;

        // Occupy this entity index
        entities[entityIndex] = entity.ToEntity(Archetype.World);

        // Update global entity location to refer to this location
        location.IndexInChunk = entityIndex;
        location.Chunk = this;

        return new EntityStorageLocation(entity, entityIndex, this);
    }

    internal void RemoveEntity(StorageLocation location)
    {
        var entityIndex = location.IndexInChunk;

        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var component in components)
        {
            Array.Clear(component, entityIndex, 1);
        }

        // No work to do if there are no other entities
        EntityCount -= 1;
        if (EntityCount == 0)
        {
            entities[entityIndex] = default;
            return;
        }

        // If we did not just destroy the top entity into place then swap the top
        // entity down into this slot to keep the chunk continuous.
        if (entityIndex != EntityCount)
        {
            var lastEntity = entities[EntityCount];
            var lastEntityIndex = EntityCount;
            ref var lastInfo = ref Archetype.World.GetStorageLocation(lastEntity.EntityId);
            entities[entityIndex] = lastEntity;
            entities[lastEntityIndex] = default;
            lastInfo.IndexInChunk = entityIndex;

            // Copy top entity components into place
            foreach (var component in components)
            {
                Array.Copy(component, lastEntityIndex, component, entityIndex, 1);

                // Clear out the components we just moved. This prevents chunks holding
                // onto references to dead managed components, and keeping them in memory.
                Array.Clear(component, lastEntityIndex, 1);
            }
        }
    }

    internal EntityStorageLocation MigrateTo(EntityId entity, ref StorageLocation location, Archetype to)
    {
        // Copy current location so we can use it later
        var srcLocation = location;

        // Move the entity to the new archetype
        var dstLocation = to.AddEntity(entity, ref location);
        var dstChunk = dstLocation.Chunk;

        // Copy across everything that exists in the destination archetype
        for (var i = 0; i < components.Length; i++)
        {
            var id = ComponentIdByComponentIndex[i].Value;

            // Check if the component is not in the destination, in which case just don't copy it
            if (id >= dstChunk.ComponentIndexByComponentId.Length || dstChunk.ComponentIndexByComponentId[id] == -1)
            {
                continue;
            }

            // Get the two arrays
            var srcArray = components[i];
            var dstArray = dstChunk.components[dstChunk.ComponentIndexByComponentId[id]];

            // Copy!
            Array.Copy(srcArray, srcLocation.IndexInChunk, dstArray, dstLocation.IndexInChunk, 1);
        }

        // Remove the entity from this chunk (using the old saved location)
        RemoveEntity(srcLocation);

        return dstLocation;
    }

    #endregion
}
