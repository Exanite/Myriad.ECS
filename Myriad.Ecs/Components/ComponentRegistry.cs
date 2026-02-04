using System;
using System.Collections.Generic;
using Exanite.Core.Runtime;
using Exanite.Core.Threading;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Stores information about component IDs.
/// </summary>
internal static class ComponentRegistry
{
    private static readonly RwLock<State> Lock = new(new State());

    /// <summary>
    /// Gets the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId<T>() where T : IComponent
    {
        var type = typeof(T);

        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetComponentId(type, out var id))
            {
                return id;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Gets the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId(Type type)
    {
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetComponentId(type, out var id))
            {
                return id;
            }
        }

        EnsureIsComponentType(type);
        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Gets the type for a given component ID.
    /// </summary>
    public static Type GetComponentType(ComponentId id)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetComponentType(id);
        }
    }

    /// <summary>
    /// Gets the dispatcher for a given component ID.
    /// </summary>
    internal static ComponentDispatcher GetComponentDispatcher(ComponentId id)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetComponentDispatcher(id);
        }
    }

    private static void EnsureIsComponentType(Type type)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type `{type.FullName}` is not assignable to `{nameof(IComponent)}`)");
        }
    }

    private class State
    {
        private readonly List<Type> typesByComponentId = [];
        private readonly List<ComponentDispatcher> dispatchersByComponentId = [];

        private readonly Dictionary<Type, ComponentId> componentIdByType = [];

        // 0 represents an invalid ID, so 1 is the first valid ID
        private int nextId = 1;

        public ComponentId GetOrAddComponentId(Type type)
        {
            if (!componentIdByType.TryGetValue(type, out var componentId))
            {
                GuardUtility.IsTrue(!type.IsClass, "Components cannot be classes");

                // Get component ID
                componentId = new ComponentId(nextId);
                nextId++;

                // Store for lookups
                typesByComponentId.Add(type);
                componentIdByType[type] = componentId;

                // Initialize the component dispatcher for this type
                var dispatcherType = typeof(ComponentDispatcher<>).MakeGenericType(type);
                var untypedDispatcher = Activator.CreateInstance(dispatcherType);
                if (untypedDispatcher is not ComponentDispatcher dispatcher)
                {
                    throw new GuardException($"Failed to create component dispatcher for type: {type}");
                }

                dispatchersByComponentId.Add(dispatcher);

                // Raise component id registered event
                ComponentId.NotifyComponentIdRegistered(componentId);
            }

            return componentId;
        }

        public bool TryGetComponentId(Type type, out ComponentId id)
        {
            return componentIdByType.TryGetValue(type, out id);
        }

        public Type GetComponentType(ComponentId id)
        {
            return typesByComponentId[id.Value - 1];
        }

        public ComponentDispatcher GetComponentDispatcher(ComponentId id)
        {
            return dispatchersByComponentId[id.Value - 1];
        }
    }
}
