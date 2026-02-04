using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// Uses the values of an immutable ordered list set as a set.
/// </summary>
internal readonly struct ComponentIdImmutableOrderedListSet : IComponentIdSet
{
    private readonly ImmutableOrderedListSet<ComponentId> componentIds;

    public ComponentIdImmutableOrderedListSet(ImmutableOrderedListSet<ComponentId> componentIds)
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
        return componentIds;
    }

    public bool SetEquals(ImmutableOrderedListSet<ComponentId> other)
    {
        return other.SetEquals(componentIds);
    }
}