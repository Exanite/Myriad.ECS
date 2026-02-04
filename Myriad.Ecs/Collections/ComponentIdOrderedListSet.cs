using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// Uses the values of an ordered list set as a set.
/// </summary>
internal readonly struct ComponentIdOrderedListSet : IComponentIdSet
{
    private readonly OrderedListSet<ComponentId> componentIds;

    public ComponentIdOrderedListSet(OrderedListSet<ComponentId> componentIds)
    {
        this.componentIds = componentIds;
    }

    public ArchetypeHash CreateArchetypeHash()
    {
        var hash = new ArchetypeHash();
        foreach (var componentId in componentIds)
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