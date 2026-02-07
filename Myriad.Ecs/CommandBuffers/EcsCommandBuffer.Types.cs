using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private readonly struct CommandState
    {
        public readonly List<EntityId> EntitiesToDestroy = [];
        public readonly List<Archetype> ArchetypesToDestroy = [];
        public readonly Dictionary<EntityId, EntityState> EntityStates = [];
        public readonly List<Action> Actions = [];

        public readonly PrefabEntityTargetLookup Lookup = new();
        public readonly ComponentSetterCollection Setters = new();

        public CommandState() {}

        public void Clear(EcsWorld world, bool hasExecuted)
        {
            if (!hasExecuted)
            {
                // Release used entity IDs
                // Do not reuse these without releasing since external callers already have access to them
                foreach (var (entityId, entityState) in EntityStates)
                {
                    if (entityState.NeedsCreation)
                    {
                        world.Entities.ReleaseId(entityId);
                    }
                }
            }

            // Release pooled entity state collections
            foreach (var entityState in EntityStates.Values)
            {
                entityState.Release();
            }

            EntitiesToDestroy.Clear();
            ArchetypesToDestroy.Clear();
            EntityStates.Clear();
            Actions.Clear();

            Lookup.Clear();
            Setters.Clear();
        }
    }

    private readonly struct SetAction : ComponentDispatcher.IComponentAction<SetAction.Input>
    {
        private readonly EcsCommandBuffer commandBuffer;

        public SetAction(EcsCommandBuffer commandBuffer)
        {
            this.commandBuffer = commandBuffer;
        }

        public void Invoke<T>(Input input) where T : IComponent
        {
            commandBuffer.Set(input.Entity, (T)input.Value);
        }

        public readonly record struct Input(Entity Entity, object Value);
    }

    private class PrefabEntityTargetLookup : IEntityLookup
    {
        private readonly Dictionary<(Entity Prefab, EntityId CurrentEntity, Entity GroupKey), Entity> perEntity = [];
        private readonly Dictionary<(Entity Prefab, Entity GroupKey), Entity> perGroup = [];
        private readonly Dictionary<Entity, Entity> global = [];

        private EntityId currentEntity;
        private Entity groupKey;

        public void Add(Entity prefab, Entity target, Entity groupKey)
        {
            perEntity.TryAdd((prefab, target.EntityId, groupKey), target);
            perGroup.TryAdd((prefab, groupKey), target);
            global.TryAdd(prefab, target);
        }

        public void SetContext(EntityId currentEntity, Entity groupKey)
        {
            this.currentEntity = currentEntity;
            this.groupKey = groupKey;
        }

        public EcsRef<T> Get<T>(EcsRef<T> from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent
        {
            return new EcsRef<T>(Get(from.Entity, policy));
        }

        public Entity Get(Entity from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist)
        {
            if (perEntity.TryGetValue((from, currentEntity, groupKey), out var to))
            {
                return to;
            }

            if (perGroup.TryGetValue((from, groupKey), out to))
            {
                return to;
            }

            if (global.TryGetValue(from, out to))
            {
                return to;
            }

            return EntityLookupUtility.HandleLookupPolicy(from, policy);
        }

        public void Clear()
        {
            perEntity.Clear();
            perGroup.Clear();
            global.Clear();
        }
    }

    private struct EntityState
    {
        public bool NeedsCreation;
        public Dictionary<ComponentId, SetterId>? Sets;
        public OrderedListSet<ComponentId>? Removes;

        public Dictionary<ComponentId, SetterId> GetOrAcquireSets()
        {
            if (Sets == null)
            {
                Sets = SimplePool<Dictionary<ComponentId, SetterId>>.Acquire();
                Sets.Clear();
            }

            return Sets;
        }

        public OrderedListSet<ComponentId> GetOrAcquireRemoves()
        {
            if (Removes == null)
            {
                Removes = SimplePool<OrderedListSet<ComponentId>>.Acquire();
                Removes.Clear();
            }

            return Removes;
        }

        public void Release()
        {
            if (Sets != null)
            {
                SimplePool<Dictionary<ComponentId, SetterId>>.Release(Sets);
            }

            if (Removes != null)
            {
                SimplePool<OrderedListSet<ComponentId>>.Release(Removes);
            }
        }
    }

    private readonly struct SetterId
    {
        private readonly int index;

        /// <summary>
        /// Component ID of the component being overwritten.
        /// </summary>
        public readonly ComponentId ComponentId;

        /// <summary>
        /// The group key used to lookup entity references.
        /// </summary>
        /// <remarks>
        /// This isn't actually used by the setter, but is used by
        /// <see cref="PrefabEntityTargetLookup"/> after the setter is applied.
        /// Main reason for this being here is that this needs to be stored
        /// on a per setter basis.
        /// </remarks>
        public readonly Entity PrefabGroupKey;

        /// <summary>
        /// If <see cref="IsPrefab"/> is false,
        /// then this is the index of the value in the corresponding components list.
        /// </summary>
        public int ValueIndex => index;

        /// <summary>
        /// If <see cref="IsPrefab"/> is true,
        /// then this is the index of the source entity in the prefabs list.
        /// </summary>
        public int PrefabIndex => ~index;

        /// <summary>
        /// Whether this setter is valid or not.
        /// </summary>
        public bool IsValid => ComponentId.Value != 0;

        /// <summary>
        /// Whether the setter reads from a prefab or not.
        /// </summary>
        public bool IsPrefab => index < 0;

        private SetterId(ComponentId componentId, int index, Entity prefabGroupKey)
        {
            ComponentId = componentId;
            this.index = index;
            PrefabGroupKey = prefabGroupKey;
        }

        public static SetterId FromValueIndex(ComponentId componentId, int index)
        {
            return new SetterId(componentId, index, default);
        }

        public static SetterId FromPrefabIndex(ComponentId componentId, int index, Entity groupKey)
        {
            return new SetterId(componentId, ~index, groupKey);
        }
    }

    /// <summary>
    /// Contains values of components to be set onto entities.
    /// Involves no boxing.
    /// </summary>
    private class ComponentSetterCollection
    {
        private readonly Dictionary<ComponentId, IComponentList> components = [];
        private readonly List<Entity> prefabs = [];
        private readonly Dictionary<Entity, int> prefabLookup = [];

        /// <summary>
        /// Creates a setter using a component value.
        /// </summary>
        public void CreateFromValue<T>(T value, ref SetterId setterId) where T : IComponent
        {
            if (setterId.IsValid && !setterId.IsPrefab)
            {
                ((ComponentList<T>)components[setterId.ComponentId]).Replace(value, setterId.ValueIndex);

                return;
            }

            var componentId = ComponentId.Get<T>();
            if (!components.TryGetValue(componentId, out var list))
            {
                components[componentId] = list = SimplePool<ComponentList<T>>.Acquire();
            }

            var index = ((ComponentList<T>)list).Create(value);
            setterId = SetterId.FromValueIndex(componentId, index);
        }

        /// <summary>
        /// Creates a setter using a component stored by a prefab entity.
        /// </summary>
        public void CreateFromPrefab(Entity prefab, ComponentId componentId, Entity groupKey, ref SetterId setterId)
        {
            AssertUtility.IsFalse(groupKey.IsDefault, "Group key can't be a default entity");

            var prefabIndex = prefabLookup.GetValueOrDefault(prefab, -1);
            if (prefabIndex == -1)
            {
                prefabIndex = prefabs.Count;
                prefabs.Add(prefab);
                prefabLookup[prefab] = prefabIndex;
            }

            setterId = SetterId.FromPrefabIndex(componentId, prefabIndex, groupKey);
        }

        /// <summary>
        /// Output the stored component value to the specified entity location.
        /// </summary>
        public void Write(SetterId setterId, EntityLocation dstLocation)
        {
            if (setterId.IsPrefab)
            {
                var srcEntity = prefabs[setterId.PrefabIndex];
                ref var srcLocation = ref srcEntity.World.Entities.GetLocation(srcEntity.EntityId);

                var srcComponents = srcLocation.Archetype.GetComponentArray(setterId.ComponentId);
                var dstComponents = dstLocation.Archetype.GetComponentArray(setterId.ComponentId);

                Array.Copy(srcComponents, srcLocation.IndexInArchetype, dstComponents, dstLocation.IndexInArchetype, 1);
            }
            else
            {
                var list = components[setterId.ComponentId];
                list.Write(setterId.ValueIndex, dstLocation);
            }
        }

        /// <summary>
        /// Clear all component values stored in this collection.
        /// </summary>
        public void Clear()
        {
            foreach (var (_, value) in components)
            {
                value.Recycle();
            }

            components.Clear();
            prefabs.Clear();
            prefabLookup.Clear();
        }

        private interface IComponentList
        {
            public void Write(int index, EntityLocation location);
            public void Recycle();
        }

        private class ComponentList<T> : IComponentList where T : IComponent
        {
            private readonly List<T> values = [];

            public int Create(T value)
            {
                values.Add(value);
                return values.Count - 1;
            }

            public void Replace(T value, int index)
            {
                values[index] = value;
            }

            public void Recycle()
            {
                values.Clear();
                SimplePool.Release(this);
            }

            public void Write(int index, EntityLocation location)
            {
                location.Archetype.Get<T>(location.IndexInArchetype) = values[index];
            }
        }
    }
}
