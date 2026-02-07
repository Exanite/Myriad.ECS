using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Buffers up modifications to entities and replays them all at once.
/// Events for an entity are reported once all modifications to the entity are completed.
/// <para/>
/// Warning: The command buffer is allowed to reorder and merge operations together.
/// <br/>
/// Example 1: Setting the same component twice is the same as setting it once, with the last taking priority.
/// <br/>
/// Example 2: Destroying an entity and making modifications to it is the same as simply destroying it.
/// </summary>
/// <remarks>
/// The command buffer batches commands to avoid expensive operations
/// where adding/removing multiple components causes the entity to be
/// copied between multiple archetypes.
/// <para/>
/// Fully ordered events without dropping intermediate events was considered,
/// but the performance cost is likely too high.
/// <para/>
/// If fully ordered events are required, it's possible to submit commands
/// in smaller batches. This has similar cost to replaying commands without
/// merging.
/// </remarks>
public sealed partial class EcsCommandBuffer
{
    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying.
    /// </summary>
    public EcsWorld World { get; }

    public bool HasBufferedOperations { get; private set; }
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// Contains information about commands enqueued in the command buffer.
    /// </summary>
    private readonly CommandState state = new();

    /// <summary>
    /// A pool of local IDs.
    /// These are bulk acquired to avoid thread contention.
    /// </summary>
    private readonly EntityId[] localIdPool = new EntityId[EcsConstants.CommandBufferLocalIdCount];

    /// <summary>
    /// The index of the next ID to read from the local ID pool.
    /// If this is greater than or equal to the count, this means that the pool is exhausted and should be refilled.
    /// </summary>
    private int nextLocalIdPoolIndex = EcsConstants.CommandBufferLocalIdCount;

    /// <summary>
    /// Create a new <see cref="EcsCommandBuffer"/> for the given <see cref="World"/>.
    /// </summary>
    internal EcsCommandBuffer(EcsWorld world)
    {
        World = world;
    }

    /// <summary>
    /// Create a <see cref="BufferedEntity"/> using the specified entity.
    /// This allows for fluent method chaining.
    /// </summary>
    public BufferedEntity Use(Entity entity)
    {
        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> in the world.
    /// </summary>
    public BufferedEntity Create()
    {
        EnsureIsExternallyMutable();

        if (nextLocalIdPoolIndex >= localIdPool.Length)
        {
            // Bulk acquire IDs if none available locally
            // This is to avoid thread contention
            World.Entities.AcquireIds(localIdPool);
            nextLocalIdPoolIndex = 0;
        }

        // Acquire an ID
        var entityId = localIdPool[nextLocalIdPoolIndex++];

        // Store the command
        ref var entityState = ref GetEntityState(entityId);
        entityState.NeedsCreation = true;

        HasBufferedOperations = true;

        return new BufferedEntity(entityId.ToEntity(World), this);
    }

    /// <inheritdoc cref="CopyFromInternal"/>
    public BufferedEntity CopyFrom(Entity entity, Entity prefab)
    {
        return CopyFromInternal(entity, prefab, entity);
    }

    /// <inheritdoc cref="CopyFromInternal"/>
    public BufferedEntity CopyFrom(Entity entity, Entity prefab, Entity groupKey)
    {
        return CopyFromInternal(entity, prefab, groupKey);
    }

    /// <summary>
    /// Copies all components from the target prefab onto the specified entity.
    /// </summary>
    /// <remarks>
    /// The component types to copy are read at the time of recording; however, the component values are read at time of execution.
    /// It is assumed that the prefab entity is not modified between now and when the command buffer is executed.
    /// </remarks>
    /// <param name="entity">The target entity.</param>
    /// <param name="prefab">The prefab to copy components from.</param>
    /// <param name="groupKey">
    /// The entity used to group entity lookups.
    /// Set this to the root entity of the hierarchy (or any consistent entity)
    /// when spawning multiple hierarchies of entities that share prefabs.
    /// </param>
    private BufferedEntity CopyFromInternal(Entity entity, Entity prefab, Entity groupKey)
    {
        EnsureIsExternallyMutable();

        // Store the commands
        ref var entityState = ref GetEntityState(entity.EntityId);
        var sets = entityState.GetOrAcquireSets();
        foreach (var componentId in prefab.ComponentIds)
        {
            ref var setterId = ref CollectionsMarshal.GetValueRefOrAddDefault(sets, componentId, out _);
            state.Setters.CreateFromPrefab(prefab, componentId, groupKey, ref setterId);

            // Prevent the remove, if it exists
            entityState.Removes?.Remove(componentId);
        }

        // Store the prefab for lookup purposes
        state.Lookup.Add(prefab, entity, groupKey);

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public BufferedEntity Set<T>(Entity entity, T value) where T : IComponent
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        var componentId = ComponentId.Get<T>();

        // Store the command
        ref var entityState = ref GetEntityState(entity.EntityId);

        var sets = entityState.GetOrAcquireSets();
        ref var setterId = ref CollectionsMarshal.GetValueRefOrAddDefault(sets, componentId, out _);
        state.Setters.CreateFromValue(value, ref setterId);

        // Prevent the remove, if it exists
        entityState.Removes?.Remove(componentId);

        HasBufferedOperations = true;

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public BufferedEntity SetBoxed(Entity entity, object value)
    {
        var setAction = new SetAction(this);
        var componentId = ComponentId.Get(value.GetType());
        var dispatcher = ComponentRegistry.GetComponentDispatcher(componentId);

        dispatcher.Invoke(setAction, new SetAction.Input(entity, value));

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Remove a component attached to an entity.
    /// </summary>
    public BufferedEntity Remove<T>(Entity entity) where T : IComponent
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        // Store the command
        var componentId = ComponentId.Get<T>();
        ref var entityState = ref GetEntityState(entity.EntityId);
        var removes = entityState.GetOrAcquireRemoves();
        removes.Add(componentId);

        // Prevent the set, if it exists
        entityState.Sets?.Remove(componentId);

        HasBufferedOperations = true;

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Destroy an entity.
    /// </summary>
    public EcsCommandBuffer Destroy(Entity entity)
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        // Store the command
        ref var entityState = ref GetEntityState(entity.EntityId);
        entityState.NeedsCreation = false;
        state.EntitiesToDestroy.Add(entity.EntityId);

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Bulk destroy entities.
    /// </summary>
    public EcsCommandBuffer Destroy(ReadOnlySpan<Entity> entities)
    {
        EnsureIsExternallyMutable();
        foreach (var entity in entities)
        {
            EnsureIsFromCurrentWorld(entity);
        }

        // Store the commands
        foreach (var entity in entities)
        {
            ref var entityState = ref GetEntityState(entity.EntityId);
            entityState.NeedsCreation = false;
            state.EntitiesToDestroy.Add(entity.EntityId);
        }

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Bulk destroy all entities in archetypes stored by the view.
    /// </summary>
    /// <remarks>
    /// The archetypes to destroy are read at the time of recording.
    /// It is assumed that no new archetypes are added between now and when the command buffer is executed.
    /// </remarks>
    public EcsCommandBuffer Destroy(IArchetypeView view)
    {
        EnsureIsExternallyMutable();

        // Capture the archetypes span first
        var archetypes = view.Archetypes;
        foreach (var archetype in archetypes)
        {
            EnsureIsFromCurrentWorld(archetype);
        }

        // Store the commands
        foreach (var archetype in archetypes)
        {
            state.ArchetypesToDestroy.Add(archetype);
        }

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Defers the specified action until the command buffer is executed.
    /// </summary>
    public EcsCommandBuffer Defer(Action action)
    {
        EnsureIsExternallyMutable();

        // Store the command
        state.Actions.Add(action);

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Clear this <see cref="EcsCommandBuffer"/>.
    /// </summary>
    public void Clear()
    {
        EnsureIsExternallyMutable();
        if (!HasBufferedOperations)
        {
            return;
        }

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool.AsSpan()[nextLocalIdPoolIndex..]);
        nextLocalIdPoolIndex = localIdPool.Length;

        // Clear commands
        state.Clear(World, false);

        HasBufferedOperations = false;
    }

    /// <summary>
    /// Apply all buffered operations to the <see cref="World"/>.
    /// </summary>
    public void Execute()
    {
        EnsureIsExternallyMutable();
        if (!HasBufferedOperations)
        {
            return;
        }

        IsExecuting = true;
        try
        {
            World.OnSyncPoint();
            ExecuteInternal();
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Used to check if it is safe for external calls to modify the command buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsExternallyMutable()
    {
        GuardUtility.IsFalse(World.IsDisposed, "World has been disposed");
        GuardUtility.IsFalse(IsExecuting, "Command buffer is currently executing");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsFromCurrentWorld(Entity entity)
    {
        GuardUtility.IsTrue(entity.World == World, "Entity must belong to the same world as the command buffer");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsFromCurrentWorld(Archetype archetype)
    {
        GuardUtility.IsTrue(archetype.World == World, "Archetype must belong to the same world as the command buffer");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref EntityState GetEntityState(EntityId entityId)
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(state.EntityStates, entityId, out var _);
    }
}
