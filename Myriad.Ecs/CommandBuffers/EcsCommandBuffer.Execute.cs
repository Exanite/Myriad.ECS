using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    /// <remarks>
    /// Stores temporary data. Clear before use.
    /// </remarks>
    private readonly OrderedListSet<ComponentId> tempComponentSet = [];

    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);
        try
        {
            DestroyEntities(recursiveCommandBuffer, state.ArchetypesToDestroy, state.EntitiesToDestroy);
            CreateAndApplyStructuralChanges(recursiveCommandBuffer, state.EntityStates);

            foreach (var action in state.Actions)
            {
                action.Invoke();
            }
        }
        finally
        {
            // Release unused local IDs
            World.Entities.ReleaseUnusedIds(localIdPool.AsSpan()[nextLocalIdPoolIndex..]);
            nextLocalIdPoolIndex = localIdPool.Length;

            // Clear commands
            state.Clear(World, true);

            HasBufferedOperations = false;
        }

        // Run any newly enqueued commands
        recursiveCommandBuffer.Execute();
    }

    private void DestroyEntities(EcsCommandBuffer recursiveCommandBuffer, List<Archetype> archetypes, List<EntityId> entities)
    {
        foreach (var archetype in archetypes)
        {
            DestroyArchetypeEntities(recursiveCommandBuffer, archetype);
        }

        foreach (var entity in entities)
        {
            DestroyEntity(recursiveCommandBuffer, entity);
        }
    }

    private void DestroyArchetypeEntities(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype)
    {
        if (archetype.EntityCount == 0)
        {
            return;
        }

        // Mark entities as dead and send events
        foreach (var chunk in archetype.Chunks)
        {
            foreach (var entity in chunk.Entities)
            {
                // Raise component removed events
                foreach (var componentId in entity.ComponentIds)
                {
                    var dispatcher = archetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                    dispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
                }

                // Raise entity destroyed event
                World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

                // Release ID
                World.Entities.ReleaseId(entity.EntityId);
            }
        }

        // Clear the archetype
        archetype.Clear();
    }

    private void DestroyEntity(EcsCommandBuffer recursiveCommandBuffer, EntityId entityId)
    {
        ref var location = ref World.Entities.GetLocation(entityId.Index);
        if (location.Version != entityId.Version)
        {
            // Ignore destroyed entities
            return;
        }

        // Raise component removed events
        var entity = entityId.ToEntity(World);
        var archetype = location.Chunk.Archetype;
        foreach (var componentId in archetype.Components)
        {
            var dispatcher = archetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
            dispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
        }

        // Raise entity destroyed event
        World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

        // Notify archetype this entity is dead
        location.Chunk.Archetype.RemoveEntity(location);

        // Release ID
        World.Entities.ReleaseId(entityId);
    }

    private void CreateAndApplyStructuralChanges(EcsCommandBuffer recursiveCommandBuffer, Dictionary<EntityId, EntityState> entityStates)
    {
        foreach (var (entityId, entityState) in entityStates)
        {
            ref var location = ref World.Entities.GetLocation(entityId.Index);
            if (location.Version != entityId.Version)
            {
                // Ignore destroyed entities
                continue;
            }

            var archetypeHash = new ArchetypeHash();
            var componentIdSet = tempComponentSet;
            componentIdSet.Clear();

            if (!entityState.NeedsCreation)
            {
                // Add existing components to set
                var archetype = location.Chunk.Archetype;

                archetypeHash = archetype.Hash;
                componentIdSet.UnionWith(archetype.Components);
            }

            // Consider component changes
            var setChanged = false;
            {
                // Component sets
                if (entityState.Sets != null)
                {
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        if (componentIdSet.Add(componentId))
                        {
                            archetypeHash = archetypeHash.Toggle(componentId);
                            setChanged = true;
                        }
                    }
                }

                // Component removes
                if (entityState.Removes != null)
                {
                    foreach (var componentId in entityState.Removes)
                    {
                        if (componentIdSet.Remove(componentId))
                        {
                            archetypeHash = archetypeHash.Toggle(componentId);
                            setChanged = true;
                        }
                    }
                }
            }

            // Case 1: Entity created
            var entity = entityId.ToEntity(World);
            if (entityState.NeedsCreation)
            {
                // Create the entity
                var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);
                dstArchetype.AddEntity(entityId, ref location);

                // Write component values
                if (entityState.Sets != null)
                {
                    foreach (var setter in entityState.Sets.Values)
                    {
                        state.Setters.Write(setter, location);
                    }
                }

                // Raise component added/copied events
                if (entityState.Sets != null)
                {
                    foreach (var (componentId, setterId) in entityState.Sets)
                    {
                        var dispatcher = dstArchetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                        if (setterId.IsPrefab)
                        {
                            state.Lookup.SetContext(entityId, setterId.PrefabGroupKey);
                            dispatcher.OnComponentCopied(recursiveCommandBuffer, entity, state.Lookup);
                        }
                        else
                        {
                            dispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                        }
                    }
                }

                // Raise entity created event
                World.EventBus.Raise(new EntityCreatedEvent(recursiveCommandBuffer, entity));

                continue;
            }

            // Case 2: Entity moved
            if (setChanged)
            {
                var srcArchetype = location.Chunk.Archetype;
                var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);

                // Raise component removed events
                if (entityState.Removes != null)
                {
                    foreach (var componentId in entityState.Removes)
                    {
                        if (srcArchetype.Components.Contains(componentId))
                        {
                            var dispatcher = srcArchetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                            dispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
                        }

                    }
                }

                // Migrate the entity
                srcArchetype.MigrateEntity(entityId, dstArchetype, ref location);

                // Write component values
                WriteComponentValues(entityState, location);

                // Raise component added/modified events
                if (entityState.Sets != null)
                {
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        var dispatcher = dstArchetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                        if (srcArchetype.Components.Contains(componentId))
                        {
                            dispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                        }
                        else
                        {
                            dispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                        }
                    }
                }

                continue;
            }

            // Case 3: Entity unmoved
            {
                // Write component values
                WriteComponentValues(entityState, location);

                // Raise component modified events
                if (entityState.Sets != null)
                {
                    var dstArchetype = location.Chunk.Archetype;
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        var dispatcher = dstArchetype.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                        dispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                    }
                }
            }
        }
    }

    private void WriteComponentValues(EntityState entityState, EntityLocation location)
    {
        if (entityState.Sets != null)
        {
            foreach (var setter in entityState.Sets.Values)
            {
                state.Setters.Write(setter, location);
            }
        }
    }
}
