using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Myriad.Ecs.xxHash;

namespace Exanite.Myriad.Ecs.Worlds.Archetypes;

/// <summary>
/// An archetype hash is made by hashing all the components in an archetype.
/// Components can be "toggled" to update the hash to a new value for an archetype with/without those components.
/// </summary>
internal readonly record struct ArchetypeHash : IComparable<ArchetypeHash>
{
    public long Value { get; private init; }

    /// <summary>
    /// Toggle (add or remove) the given component
    /// </summary>
    public ArchetypeHash Toggle(ComponentId component)
    {
        return new ArchetypeHash
        {
            Value = Toggle(Value, component),
        };
    }

    private static long Toggle(long value, ComponentId component)
    {
        unsafe
        {
            // Hash component value to smear bits across 64 bit hash space
            var cv = component.Value;
            var v = unchecked((long)xxHash64.ComputeHash(new Span<byte>(&cv, 4), 17));

            // xor this value to toggle it in the set
            return value ^ v;
        }
    }

    public override string ToString()
    {
        return $"0x{Value:X16}";
    }

    internal static ArchetypeHash Create(OrderedListSet<ComponentId> componentIds)
    {
        long l = 0;
        foreach (var componentId in componentIds)
        {
            l = Toggle(l, componentId);
        }

        return new ArchetypeHash { Value = l };
    }

    public static ArchetypeHash Create<TV>(Dictionary<ComponentId, TV> componentIds)
    {
        long l = 0;
        foreach (var componentId in componentIds.Keys)
        {
            l = Toggle(l, componentId);
        }

        return new ArchetypeHash { Value = l };
    }

    public int CompareTo(ArchetypeHash other)
    {
        return Value.CompareTo(other.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
