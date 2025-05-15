using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Unique numeric ID for a type which implements IComponent.
/// </summary>
[DebuggerDisplay("{Type} ({Value})")]
public readonly record struct ComponentId : IComparable<ComponentId>
{
    private static List<ComponentId> registeredComponentIds = new();

    /// <summary>
    /// All component IDs that have been discovered and registered so far.
    /// </summary>
    public static IReadOnlyList<ComponentId> RegisteredComponentIds => registeredComponentIds;

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

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="type"/> does not implement <see cref="IComponent"/>.</exception>
    public static ComponentId Get(Type type)
    {
        return ComponentRegistry.GetComponentId(type);
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    public static ComponentId Get<T>() where T : IComponent
    {
        return ComponentRegistry.GetComponentId<T>();
    }

    internal static void NotifyComponentIdRegistered(ComponentId componentId)
    {
        registeredComponentIds.Add(componentId);
        ComponentIdRegistered?.Invoke(componentId);
    }
}
