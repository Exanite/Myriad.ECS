using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// This is used to manually create generic specializations for types that don't share an interface.
/// </summary>
/// <remarks>
/// This is a bit clunky to use so only use this for internal APIs.
/// </remarks>
internal interface IComponentIdSet
{
    public ArchetypeHash CreateArchetypeHash();

    public ImmutableOrderedListSet<ComponentId> ToImmutableOrderedListSet();

    public bool SetEquals(ImmutableOrderedListSet<ComponentId> other);
}
