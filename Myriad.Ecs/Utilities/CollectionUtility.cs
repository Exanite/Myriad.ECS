using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Utilities;

internal static class CollectionUtility
{
    public static ComponentBloomFilter ToBloomFilter(this ImmutableOrderedListSet<ComponentId> set)
    {
        var filter = new ComponentBloomFilter();
        foreach (var item in set)
        {
            filter.Add(item);
        }

        return filter;
    }
}
