using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

internal record struct QueryCacheKey(
    ImmutableOrderedListSet<ComponentId> IncludeFilter,
    ImmutableOrderedListSet<ComponentId> ExcludeFilter,
    ImmutableOrderedListSet<ComponentId> AtLeastOneFilter,
    ImmutableOrderedListSet<ComponentId> ExactlyOneFilter,
    ImmutableOrderedListSet<ComponentId> NotAllFilter);
