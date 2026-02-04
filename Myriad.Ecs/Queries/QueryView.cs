using System;
using System.Collections.Generic;
using Exanite.Core.Threading;
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
    /// <summary>
    /// Cached result.Value from the last time <see cref="GetArchetypeMatchResult"/> was called.
    /// </summary>
    private readonly RwLock<ArchetypeMatches?> resultLock = new(null);
    private readonly OrderedListSet<ComponentId> temporarySet = [];

    private readonly ComponentBloomFilter includeBloom;
    private readonly ComponentBloomFilter excludeBloom;

    /// <summary>
    /// The <see cref="EcsWorld"/> that this query is for.
    /// </summary>
    public EcsWorld World { get; }

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
    public ReadOnlySpan<Archetype> Archetypes => GetArchetypeMatchResult().Archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => GetArchetypeMatchResult().Archetypes;

    /// <summary>
    /// Checks if an entity matches this query.
    /// </summary>
    public bool IsMatch(Entity entity)
    {
        if (!entity.IsAlive)
        {
            return false;
        }

        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(entity.World.Entities.GetArchetype(entity.EntityId));
    }

    /// <summary>
    /// Checks if a chunk matches this query.
    /// </summary>
    public bool IsMatch(Chunk chunk)
    {
        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(chunk.Archetype);
    }

    /// <summary>
    /// Checks if an archetype matches this query.
    /// </summary>
    public bool IsMatch(Archetype archetype)
    {
        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(archetype);
    }

    private ArchetypeMatches GetArchetypeMatchResult()
    {
        // Quickly check if we already have a non-stale result
        using (resultLock.EnterReadLock(out var result))
        {
            if (result.Value != null && !result.Value.Value.IsStale(World))
            {
                return result.Value.Value;
            }
        }

        // We don't have a valid cached result, calculate it now
        using (resultLock.EnterWriteLock(out var result))
        {
            // If this query has never been evaluated before do it now
            if (result.Value == null)
            {
                // Check every archetype
                var matches = new List<ArchetypeMatch>();
                foreach (var archetype in World.Archetypes)
                {
                    if (TryMatch(archetype, out var match))
                    {
                        matches.Add(match);
                    }
                }

                // Store result for next time
                result.Value = new ArchetypeMatches(World.Archetypes.Length, ImmutableOrderedListSet<ArchetypeMatch>.Create(matches));

                // Return matches
                return result.Value.Value;
            }

            // If the number of archetypes has changed since last time regenerate the cache
            if (result.Value.Value.IsStale(World))
            {
                // Lazily allocated set of new archetype matches
                var newMatches = default(OrderedListSet<ArchetypeMatch>?);

                // Check every new archetype
                for (var i = result.Value.Value.ArchetypeWatermark; i < World.Archetypes.Length; i++)
                {
                    if (!TryMatch(World.Archetypes[i], out var match))
                    {
                        continue;
                    }

                    // Initialize new matches now that we know we need it
                    newMatches ??= new OrderedListSet<ArchetypeMatch>(result.Value.Value.ArchetypesMatches);

                    // Add the match
                    newMatches.Add(match);
                }

                if (newMatches == null)
                {
                    // Copy is null, meaning nothing new was found, just use the old result with the new watermark
                    result.Value = new ArchetypeMatches(World.Archetypes.Length, result.Value.Value.ArchetypesMatches);
                }
                else
                {
                    // Create a new match result
                    result.Value = new ArchetypeMatches(World.Archetypes.Length, ImmutableOrderedListSet<ArchetypeMatch>.Create(newMatches));
                }
            }

            return result.Value.Value;
        }
    }

    private bool TryMatch(Archetype archetype, out ArchetypeMatch match)
    {
        match = default;

        // Apply the Include filter
        // Quick bloom filter test if the included components intersects with the archetype.
        // If this returns false there is definitely no overlap at all and we can early exit.
        if (IncludeFilter.Count > 0 && !archetype.ComponentsBloomFilter.MaybeIntersects(in includeBloom))
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
        if (ExcludeFilter.Count > 0 && excludeBloom.MaybeIntersects(in archetype.ComponentsBloomFilter))
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
        match = new ArchetypeMatch(archetype);

        return true;
    }

    private readonly struct ArchetypeMatches
    {
        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public ImmutableOrderedListSet<ArchetypeMatch> ArchetypesMatches { get; }

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public List<Archetype> Archetypes { get; }

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public HashSet<Archetype> ArchetypeSet { get; }

        /// <summary>
        /// The number of archetypes in the world when this cache was created. Used for caching purposes.
        /// </summary>
        public int ArchetypeWatermark { get; }

        public ArchetypeMatches(int watermark, ImmutableOrderedListSet<ArchetypeMatch> archetypesMatches)
        {
            ArchetypesMatches = archetypesMatches;
            ArchetypeWatermark = watermark;

            Archetypes = new List<Archetype>(archetypesMatches.Count);
            foreach (var match in archetypesMatches)
            {
                Archetypes.Add(match.Archetype);
            }

            ArchetypeSet = [..Archetypes];
        }

        public bool IsStale(EcsWorld world)
        {
            return ArchetypeWatermark < world.Archetypes.Length;
        }
    }

    /// <summary>
    /// An archetype that matched a query.
    /// </summary>
    private readonly record struct ArchetypeMatch(Archetype Archetype) : IComparable<ArchetypeMatch>
    {
        /// <inheritdoc/>
        public int CompareTo(ArchetypeMatch other)
        {
            return Archetype.Hash.CompareTo(other.Archetype.Hash);
        }
    }
}
