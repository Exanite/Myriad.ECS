using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Allocations;
using Exanite.Myriad.Ecs.Threading;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Stores information about component IDs.
/// </summary>
internal static class ComponentRegistry
{
    private static readonly RwLock<State> Lock = new(new State());

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId<T>() where T : IComponent
    {
        var type = typeof(T);

        using (var locker = Lock.EnterReadLock())
        {
            if (locker.Value.TryGetComponentId(type, out var value))
            {
                return value;
            }
        }

        using (var locker = Lock.EnterWriteLock())
        {
            return locker.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId(Type type)
    {
        using (var locker = Lock.EnterReadLock())
        {
            if (locker.Value.TryGetComponentId(type, out var value))
            {
                return value;
            }
        }

        EnsureIsComponentType(type);
        using (var locker = Lock.EnterWriteLock())
        {
            return locker.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Get the type for a given component ID.
    /// </summary>
    public static Type GetComponentType(ComponentId id)
    {
        using var locker = Lock.EnterReadLock();

        if (!locker.Value.TryGetComponentType(id, out var type))
        {
            throw new InvalidOperationException("Unknown component ID");
        }

        return type;
    }

    /// <summary>
    /// Get the type for a given component ID.
    /// </summary>
    internal static ComponentEventDispatcher GetComponentEventDispatcher(ComponentId id)
    {
        using var locker = Lock.EnterReadLock();

        if (!locker.Value.TryGetComponentEventDispatcher(id, out var eventDispatcher))
        {
            throw new InvalidOperationException("Unknown component ID");
        }

        return eventDispatcher;
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
        private readonly Dictionary<ComponentId, Type> typesByComponentId = [];
        private readonly Dictionary<ComponentId, ComponentEventDispatcher> eventDispatchersByComponentId = [];

        private readonly Dictionary<Type, ComponentId> componentIdByType = [];

        // 0 represents an invalid ID, so 1 is the first valid ID
        private int nextId = 1;

        public ComponentId GetOrAddComponentId(Type type)
        {
            if (!componentIdByType.TryGetValue(type, out var componentId))
            {
                // Get component ID
                componentId = new ComponentId(nextId);
                nextId++;

                // Store for lookups
                typesByComponentId[componentId] = type;
                componentIdByType[type] = componentId;

                // Initialize the array factory for this type
                ArrayFactory.Initialize(type);

                // Initialize the event dispatcher for this type
                var eventDispatcherType = typeof(ComponentEventDispatcher<>).MakeGenericType(type);
                var untypedEventDispatcher = Activator.CreateInstance(eventDispatcherType);
                if (untypedEventDispatcher is not ComponentEventDispatcher eventDispatcher)
                {
                    throw new GuardException($"Failed to create event dispatcher for type: {type}");
                }

                eventDispatchersByComponentId[componentId] = eventDispatcher;

                // Raise component id registered event
                ComponentId.NotifyComponentIdRegistered(componentId);
            }

            return componentId;
        }

        public bool TryGetComponentId(Type type, out ComponentId id)
        {
            return componentIdByType.TryGetValue(type, out id);
        }

        public bool TryGetComponentType(ComponentId id, [MaybeNullWhen(false)] out Type type)
        {
            return typesByComponentId.TryGetValue(id, out type);
        }

        public bool TryGetComponentEventDispatcher(ComponentId id, [MaybeNullWhen(false)] out ComponentEventDispatcher eventDispatcher)
        {
            return eventDispatchersByComponentId.TryGetValue(id, out eventDispatcher);
        }
    }
}
