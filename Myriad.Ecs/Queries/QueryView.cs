using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Contains the set of matched archetypes based on a filter described by <see cref="QueryFilter"/>.
/// </summary>
public sealed class QueryView : IArchetypeView
{
    private MatchResult result = new(0, []);

    /// <summary>
    /// The <see cref="EcsWorld"/> that this query is for.
    /// </summary>
    public EcsWorld World { get; }

    private readonly Lock updateLock = new();
    private readonly ComponentBloomFilter includeBloom;
    private readonly ComponentBloomFilter excludeBloom;

    /// <summary>
    /// The components which must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> IncludeFilter { get; }

    /// <summary>
    /// The components which must not be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExcludeFilter { get; }

    /// <summary>
    /// At least one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> AtLeastOneFilter { get; }

    /// <summary>
    /// Exactly one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExactlyOneFilter { get; }

    /// <summary>
    /// Not all of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> NotAllFilter { get; }

    /// <summary>
    /// Describes a query for entities, bound to a world.
    /// </summary>
    internal QueryView(
        EcsWorld world,
        ImmutableOrderedListSet<ComponentId> includeFilter,
        ImmutableOrderedListSet<ComponentId> excludeFilter,
        ImmutableOrderedListSet<ComponentId> atLeastOneFilter,
        ImmutableOrderedListSet<ComponentId> exactlyOneFilter,
        ImmutableOrderedListSet<ComponentId> notAllFilter)
    {
        World = world;

        IncludeFilter = includeFilter;
        ExcludeFilter = excludeFilter;
        AtLeastOneFilter = atLeastOneFilter;
        ExactlyOneFilter = exactlyOneFilter;
        NotAllFilter = notAllFilter;

        includeBloom = includeFilter.ToBloomFilter();
        excludeBloom = excludeFilter.ToBloomFilter();
    }

    /// <summary>
    /// Create a <see cref="QueryFilter"/> with the same filter as this query.
    /// </summary>
    public QueryFilter ToFilter()
    {
        var filter = new QueryFilter();

        foreach (var id in IncludeFilter)
        {
            filter.Include(id);
        }

        foreach (var id in ExcludeFilter)
        {
            filter.Exclude(id);
        }

        foreach (var id in AtLeastOneFilter)
        {
            filter.AtLeastOne(id);
        }

        foreach (var id in ExactlyOneFilter)
        {
            filter.ExactlyOne(id);
        }

        foreach (var id in NotAllFilter)
        {
            filter.NotAll(id);
        }

        return filter;
    }

    /// <summary>
    /// The archetypes matched by this query.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => GetMatchResult().Archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => GetMatchResult().Archetypes;

    /// <summary>
    /// Checks if an entity matches this query.
    /// </summary>
    public bool IsMatch(Entity entity)
    {
        if (!entity.IsAlive)
        {
            return false;
        }

        return IsMatch(entity.Archetype);
    }

    /// <summary>
    /// Checks if an archetype matches this query.
    /// </summary>
    public bool IsMatch(Archetype archetype)
    {
        if (archetype.World != World)
        {
            return false;
        }

        return Archetypes.BinarySearch(archetype, new ArchetypeComparer()) >= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MatchResult GetMatchResult()
    {
        // Quickly check if we already have a non-stale result
        var currentResult = Volatile.Read(in result);
        if (currentResult.Version == Volatile.Read(in World.Version))
        {
            return currentResult;
        }

        return GetMatchResultCold(this);
    }

    /// <summary>
    /// This assumes the current result is stale.
    /// </summary>
    /// <remarks>
    /// <see cref="view"/> is passed in manually to avoid accessing instance state on accident.
    /// </remarks>
    private static MatchResult GetMatchResultCold(QueryView view)
    {
        // Allow only one thread to update at a time
        // Updates are usually incremental and fast so this is fine
        using var _ = view.updateLock.EnterScope();

        var oldResult = Volatile.Read(in view.result);

        // Lazily allocated set of new archetype matches
        var newMatches = default(List<Archetype>?);

        // Check every new archetype
        var world = view.World;
        var archetypes = world.Archetypes;
        {
            // Acquire temporary set from pool
            // No need to clear on returning since component id does not have managed references
            using var __ = SimplePool<OrderedListSet<ComponentId>>.Acquire(out var temporarySet);
            for (var i = oldResult.Version; i < archetypes.Length; i++)
            {
                var archetype = archetypes[i];
                if (!view.IsMatch(archetype, temporarySet))
                {
                    continue;
                }

                // Add the matched archetype
                if (newMatches == null)
                {
                    newMatches = ListPool<Archetype>.Acquire();
                    newMatches.AddRange(oldResult.Archetypes);
                }

                newMatches.Add(archetype);
            }
        }

        MatchResult newResult;
        if (newMatches == null)
        {
            // Copy is null, meaning nothing new was found, just use the old result with the new version
            newResult = new MatchResult(archetypes.Length, oldResult.Archetypes);
        }
        else
        {
            // Create a new match result
            newResult = new MatchResult(archetypes.Length, newMatches);

            // Defer the recycling of the old list to the world
            // The world will recycle them at the next sync point
            world.Recycle(oldResult.Archetypes);
        }

        // Replace old data
        Volatile.Write(ref view.result, newResult);

        return newResult;
    }

    private bool IsMatch(Archetype archetype, OrderedListSet<ComponentId> temporarySet)
    {
        // Apply the Include filter
        // Quick bloom filter test if the included components intersects with the archetype.
        // If this returns false there is definitely no overlap at all and we can early exit.
        if (IncludeFilter.Count > 0 && !archetype.Info.BloomFilter.MaybeIntersects(in includeBloom))
        {
            return false;
        }

        // Do the full set check for included components
        if (!archetype.Components.IsSupersetOf(IncludeFilter))
        {
            return false;
        }

        // Apply the Exclude filter
        // If this is false it means there is definitely _not_ an intersection, which means we can skip
        // the inner check.
        if (ExcludeFilter.Count > 0 && excludeBloom.MaybeIntersects(in archetype.Info.BloomFilter))
        {
            if (archetype.Components.Overlaps(ExcludeFilter))
            {
                return false;
            }
        }

        // Apply the ExactlyOne filter
        if (ExactlyOneFilter.Count > 0)
        {
            temporarySet.Clear();
            temporarySet.UnionWith(archetype.Components);
            temporarySet.IntersectWith(ExactlyOneFilter);
            if (temporarySet.Count != 1)
            {
                temporarySet.Clear();
                return false;
            }
        }

        // Apply the AtLeastOne filter
        if (AtLeastOneFilter.Count > 0)
        {
            temporarySet.Clear();
            temporarySet.UnionWith(archetype.Components);
            temporarySet.IntersectWith(AtLeastOneFilter);
            if (temporarySet.Count == 0)
            {
                temporarySet.Clear();
                return false;
            }
        }

        // Apply the NotAll filter
        if (NotAllFilter.Count > 0 && archetype.Components.IsSupersetOf(NotAllFilter))
        {
            return false;
        }

        temporarySet.Clear();
        return true;
    }

    private class MatchResult
    {
        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public readonly List<Archetype> Archetypes;

        /// <summary>
        /// The number of archetypes in the world when this cache was created. Used for caching purposes.
        /// </summary>
        public readonly int Version;

        public MatchResult(int version, List<Archetype> archetypes)
        {
            Version = version;
            Archetypes = archetypes;
        }
    }

    private readonly struct ArchetypeComparer : IComparer<Archetype>
    {
        public int Compare(Archetype? left, Archetype? right)
        {
            // Archetypes are never null when this comparer is used
            return left!.Id.CompareTo(right!.Id);
        }
    }
}
