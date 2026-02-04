using System.Collections.Generic;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// Uses the keys of a dictionary as a set.
/// </summary>
internal readonly struct ComponentIdDictionary<TValue> : IComponentIdSet
{
    private readonly Dictionary<ComponentId, TValue> componentIds;

    public ComponentIdDictionary(Dictionary<ComponentId, TValue> componentIds)
    {
        this.componentIds = componentIds;
    }

    public ArchetypeHash CreateArchetypeHash()
    {
        var hash = new ArchetypeHash();
        foreach (var componentId in componentIds.Keys)
        {
            hash = hash.Toggle(componentId);
        }

        return hash;
    }

    public ImmutableOrderedListSet<ComponentId> ToImmutableOrderedListSet()
    {
        return ImmutableOrderedListSet<ComponentId>.Create(componentIds);
    }

    public bool SetEquals(ImmutableOrderedListSet<ComponentId> other)
    {
        return other.SetEquals(componentIds);
    }
}