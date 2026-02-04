using System.Collections.Generic;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Collections;

internal static class ComponentIdSetUtility
{
    public static ComponentIdDictionary<TValue> AsComponentIdSet<TValue>(this Dictionary<ComponentId, TValue> componentIds)
    {
        return new ComponentIdDictionary<TValue>(componentIds);
    }

    public static ComponentIdOrderedListSet AsComponentIdSet(this OrderedListSet<ComponentId> componentIds)
    {
        return new ComponentIdOrderedListSet(componentIds);
    }

    public static ComponentIdImmutableOrderedListSet AsComponentIdSet(this ImmutableOrderedListSet<ComponentId> componentIds)
    {
        return new ComponentIdImmutableOrderedListSet(componentIds);
    }
    
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