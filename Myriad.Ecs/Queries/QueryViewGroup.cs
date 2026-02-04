using System;
using System.Collections.Generic;
using Exanite.Core.Threading;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Wraps a group of queries and exposes them as a single view.
/// </summary>
public class QueryViewGroup : IArchetypeView
{
    private readonly QueryView[] queries;
    private readonly RwLock<List<Archetype>> resultLock = new([]);

    /// <summary>
    /// The queries in the group.
    /// </summary>
    public ReadOnlySpan<QueryView> Queries => queries;

    /// <inheritdoc cref="Queries"/>
    public IReadOnlyList<QueryView> QueriesList => queries;

    /// <summary>
    /// The archetypes matched by this query group.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => GetArchetypes().AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => GetArchetypes();

    public QueryViewGroup(QueryFilter filter, ReadOnlySpan<EcsWorld> worlds)
    {
        queries = new QueryView[worlds.Length];
        for (var i = 0; i < worlds.Length; i++)
        {
            queries[i] = filter.Build(worlds[i]);
        }
    }

    /// <summary>
    /// Creates a <see cref="QueryViewGroup"/> when multiple worlds are provided or a <see cref="QueryView"/> when one group is provided.
    /// </summary>
    public static IArchetypeView Create(QueryFilter filter, ReadOnlySpan<EcsWorld> worlds)
    {
        if (worlds.Length == 1)
        {
            return filter.Build(worlds[0]);
        }

        return new QueryViewGroup(filter, worlds);
    }

    private List<Archetype> GetArchetypes()
    {
        var archetypeCount = 0;
        foreach (var query in queries)
        {
            archetypeCount += query.Archetypes.Length;
        }

        // Return result if not stale
        using (resultLock.EnterReadLock(out var result))
        {
            if (result.Value.Count == archetypeCount)
            {
                return result.Value;
            }
        }

        // Recalculate result
        using (resultLock.EnterWriteLock(out var result))
        {
            var newResult = new List<Archetype>(archetypeCount);
            foreach (var query in queries)
            {
                newResult.AddRange(query.Archetypes);
            }

            result.Value = newResult;
            return newResult;
        }
    }
}
