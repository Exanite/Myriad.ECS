using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Unique numeric ID for a type which implements IComponent.
/// </summary>
public readonly record struct ComponentId : IComparable<ComponentId>
{
    private static readonly ConcurrentBag<ComponentId> registeredComponentIds = new();

    /// <summary>
    /// All component IDs that have been discovered and registered so far.
    /// </summary>
    public static IReadOnlyCollection<ComponentId> RegisteredComponentIds => registeredComponentIds;

    /// <summary>
    /// Raised when a new component ID is registered. May be called from any thread.
    /// </summary>
    public static event Action<ComponentId>? ComponentIdRegistered;

    /// <summary>
    /// Get the raw value of this ID.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// The <see cref="System.Type"/> of the component this ID is for.
    /// </summary>
    public Type Type => ComponentRegistry.GetComponentType(this);

    internal ComponentId(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the dispatcher for this component ID.
    /// </summary>
    /// <remarks>
    /// The dispatcher can be used to invoke generic methods given only a component ID.
    /// </remarks>
    public ComponentDispatcher GetDispatcher()
    {
        return ComponentRegistry.GetComponentDispatcher(this);
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="type"/> does not implement <see cref="IComponent"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentId Get(Type type)
    {
        return ComponentRegistry.GetComponentId(type);
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentId Get<T>() where T : IComponent
    {
        return ComponentId<T>.Id;
    }

    /// <inheritdoc/>
    public int CompareTo(ComponentId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} ({Value})";
    }

    internal static void NotifyComponentIdRegistered(ComponentId componentId)
    {
        registeredComponentIds.Add(componentId);

        ComponentIdRegistered?.Invoke(componentId);
    }
}

internal static class ComponentId<T> where T : IComponent
{
    /// <summary>
    /// The component ID for <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This property is cached, making repeated accesses very efficient.
    /// </remarks>
    public static readonly ComponentId Id = ComponentRegistry.GetComponentId<T>();
}
