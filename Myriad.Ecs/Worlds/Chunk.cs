using System;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Worlds;

public sealed class Chunk
{
    /// <summary>
    /// The archetype which contains this chunk.
    /// </summary>
    public Archetype Archetype { get; }

    /// <inheritdoc cref="ArchetypeComponentLookup"/>
    internal readonly ArchetypeComponentLookup Lookup;

    /// <remarks>
    /// Indexed using entity index.
    /// </remarks>>
    private readonly Entity[] entityColumn;

    /// <remarks>
    /// Indexed using column index, then entity index.
    /// </remarks>>
    private readonly Array[] componentColumns;

    /// <summary>
    /// Whether the chunk is full or not.
    /// </summary>
    internal bool IsFull => EntityCount == EcsConstants.ChunkEntityCount;

    /// <summary>
    /// Get the number of entities currently in this chunk.
    /// </summary>
    private int EntityCount { get; set; }

    /// <summary>
    /// All entities in this chunk.
    /// </summary>
    public ReadOnlySpan<Entity> Entities => entityColumn.AsSpan(0, EntityCount);

    internal Chunk(Archetype archetype)
    {
        Archetype = archetype;
        Lookup = archetype.Lookup;

        entityColumn = new Entity[EcsConstants.ChunkEntityCount];
        componentColumns = new Array[Lookup.ComponentIdByColumnIndex.Length];

        var arrayFactory = new ArrayFactory();
        for (var i = 0; i < componentColumns.Length; i++)
        {
            var dispatcher = Lookup.ComponentDispatcherByComponentId[Lookup.ComponentIdByColumnIndex[i].Value];
            componentColumns[i] = dispatcher.Create<ArrayFactory, int, Array>(arrayFactory, EcsConstants.ChunkEntityCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>() where T : IComponent
    {
        return GetSpan<T>(ComponentId.Get<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>(ComponentId id) where T : IComponent
    {
        return ((T[])GetComponentArray(id)).AsSpan(0, EntityCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Array GetComponentArray(ComponentId id)
    {
        return componentColumns[Lookup.ColumnIndexByComponentId[id.Value]];
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex));
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex, ComponentId id) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex, id));
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex) where T : IComponent
    {
        return ref Get<T>(entityIndex, ComponentId.Get<T>());
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex, ComponentId id) where T : IComponent
    {
        return ref GetSpan<T>(id)[entityIndex];
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void Clear()
    {
        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var componentColumn in componentColumns)
        {
            Array.Clear(componentColumn, 0, componentColumn.Length);
        }

        // Not strictly necessary, clean up all the IDs so they're default instead of some invalid value.
        Array.Clear(entityColumn, 0, entityColumn.Length);

        EntityCount = 0;
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void CopyFrom(Chunk srcChunk, EcsCommandBuffer recursiveCommandBuffer, EntityLookup lookup)
    {
        for (var i = 0; i < srcChunk.componentColumns.Length; i++)
        {
            var srcColumn = srcChunk.componentColumns[i];
            var dstColumn = componentColumns[i];
            Array.Copy(srcColumn, dstColumn, srcChunk.EntityCount);
        }

        var world = Archetype.World;
        while (EntityCount < srcChunk.EntityCount)
        {
            // Allocate an entity id and add it to this chunk
            ref var location = ref world.Entities.AcquireId(out var entityId);
            AddEntity(entityId, ref location);

            // Add the entity pair to the lookup dictionary
            var originalEntity = srcChunk.Entities[location.IndexInChunk];
            var newEntity = entityId.ToEntity(world);
            lookup.Add(originalEntity, newEntity);

            // Raise entity created event
            world.EventBus.Raise(new EntityCreatedEvent(recursiveCommandBuffer, newEntity));
        }
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void AddEntity(EntityId entityId, ref EntityLocation location)
    {
        // It is safe to only assert here. It should never happen if Myriad is working
        // correctly. If it does somehow go wrong you'll get an index out of range exception
        // below so it still fails in a sensible way.
        AssertUtility.IsTrue(EntityCount < entityColumn.Length, "Cannot add entity to full chunk");

        // Use the next free slot
        var entityIndex = EntityCount++;

        // Store the entity in this chunk
        entityColumn[entityIndex] = entityId.ToEntity(Archetype.World);

        // Update the storage location to refer to this chunk
        location.IndexInChunk = entityIndex;
        location.Chunk = this;
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void RemoveEntity(EntityLocation location)
    {
        var entityIndex = location.IndexInChunk;

        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var componentColumn in componentColumns)
        {
            Array.Clear(componentColumn, entityIndex, 1);
        }

        // No work to do if there are no other entities
        EntityCount -= 1;
        if (EntityCount == 0)
        {
            entityColumn[entityIndex] = default;
            return;
        }

        // If we did not just destroy the top entity into place then swap the top
        // entity down into this slot to keep the chunk continuous.
        if (entityIndex != EntityCount)
        {
            var lastEntity = entityColumn[EntityCount];
            var lastEntityIndex = EntityCount;
            ref var lastInfo = ref Archetype.World.Entities.GetLocation(lastEntity.EntityId);
            entityColumn[entityIndex] = lastEntity;
            entityColumn[lastEntityIndex] = default;
            lastInfo.IndexInChunk = entityIndex;

            // Copy top entity components into place
            foreach (var componentColumn in componentColumns)
            {
                Array.Copy(componentColumn, lastEntityIndex, componentColumn, entityIndex, 1);

                // Clear out the components we just moved. This prevents chunks holding
                // onto references to dead managed components, and keeping them in memory.
                Array.Clear(componentColumn, lastEntityIndex, 1);
            }
        }
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void MigrateTo(EntityId entity, ref EntityLocation location, Archetype to)
    {
        // Copy current location so we can use it later
        var srcLocation = location;

        // Move the entity to the new archetype
        to.AddEntity(entity, ref location);
        var dstChunk = location.Chunk;

        // Copy across everything that exists in the destination archetype
        for (var i = 0; i < componentColumns.Length; i++)
        {
            var id = Lookup.ComponentIdByColumnIndex[i].Value;

            // Check if the component is not in the destination, in which case just don't copy it
            if (id >= dstChunk.Lookup.ColumnIndexByComponentId.Length || dstChunk.Lookup.ColumnIndexByComponentId[id] == -1)
            {
                continue;
            }

            // Get the two arrays
            var srcArray = componentColumns[i];
            var dstArray = dstChunk.componentColumns[dstChunk.Lookup.ColumnIndexByComponentId[id]];

            // Copy!
            Array.Copy(srcArray, srcLocation.IndexInChunk, dstArray, location.IndexInChunk, 1);
        }

        // Remove the entity from this chunk (using the old saved location)
        RemoveEntity(srcLocation);
    }

    private readonly struct ArrayFactory : ComponentDispatcher.IComponentFactory<int, Array>
    {
        public Array Create<T>(int capacity) where T : IComponent
        {
            return capacity == 0 ? [] : new T[capacity];
        }
    }
}
