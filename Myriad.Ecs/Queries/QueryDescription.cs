using System;
using System.Collections.Generic;
using System.Threading;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Utilities;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Describes a query for entities, bound to a world.
/// </summary>
public sealed class QueryDescription
{
    // Cache of result from last time TryMatch was called
    private ArchetypeMatchResult? result;
    private readonly ReaderWriterLockSlim resultLock = new();
    private readonly OrderedListSet<ComponentId> temporarySet = [];

    private readonly ComponentBloomFilter includeBloom;
    private readonly ComponentBloomFilter excludeBloom;

    /// <summary>
    /// The World that this query is for
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The components which must be present on an entity for it to match this query
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Include { get; }

    /// <summary>
    /// The components which must not be present on an entity for it to match this query
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Exclude { get; }

    /// <summary>
    /// At least one of these components must be present on an entity for it to match this query
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> AtLeastOneOf { get; }

    /// <summary>
    /// Exactly one of these components must be present on an entity for it to match this query
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExactlyOneOf { get; }

    /// <summary>
    /// Describes a query for entities, bound to a world.
    /// </summary>
    internal QueryDescription(EcsWorld world, ImmutableOrderedListSet<ComponentId> include, ImmutableOrderedListSet<ComponentId> exclude, ImmutableOrderedListSet<ComponentId> atLeastOne, ImmutableOrderedListSet<ComponentId> exactlyOne)
    {
        World = world;

        Include = include;
        Exclude = exclude;
        AtLeastOneOf = atLeastOne;
        ExactlyOneOf = exactlyOne;

        includeBloom = include.ToBloomFilter();
        excludeBloom = exclude.ToBloomFilter();
    }

    /// <summary>
    /// Create a query builder which describes this query
    /// </summary>
    public QueryBuilder ToBuilder()
    {
        var builder = new QueryBuilder();

        foreach (var id in Include)
        {
            builder.Include(id);
        }

        foreach (var id in Exclude)
        {
            builder.Exclude(id);
        }

        foreach (var id in AtLeastOneOf)
        {
            builder.AtLeastOneOf(id);
        }

        foreach (var id in ExactlyOneOf)
        {
            builder.ExactlyOneOf(id);
        }

        return builder;
    }

    #region Is In Query

    /// <summary>
    /// Check if this query requires the given component
    /// </summary>
    public bool IsIncluded<T>()
        where T : IComponent
    {
        return IsIncluded(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if this query requires the given component
    /// </summary>
    public bool IsIncluded(Type type)
    {
        return IsIncluded(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if this query requires the given component
    /// </summary>
    public bool IsIncluded(ComponentId id)
    {
        return Include.Contains(id);
    }

    /// <summary>
    /// Check if this query excludes entities with the given component
    /// </summary>
    public bool IsExcluded<T>()
        where T : IComponent
    {
        return IsExcluded(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if this query excludes entities with the given component
    /// </summary>
    public bool IsExcluded(Type type)
    {
        return IsExcluded(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if this query excludes entities with the given component
    /// </summary>
    public bool IsExcluded(ComponentId id)
    {
        return Exclude.Contains(id);
    }

    /// <summary>
    /// Check if the given component is one of the components which at least one of must be on the entity
    /// </summary>
    public bool IsAtLeastOneOf<T>()
        where T : IComponent
    {
        return IsAtLeastOneOf(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the given component is one of the components which at least one of must be on the entity
    /// </summary>
    public bool IsAtLeastOneOf(Type type)
    {
        return IsAtLeastOneOf(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the given component is one of the components which at least one of must be on the entity
    /// </summary>
    public bool IsAtLeastOneOf(ComponentId id)
    {
        return AtLeastOneOf.Contains(id);
    }

    /// <summary>
    /// Check if the given component is one of the components which exactly one of must be on the entity
    /// </summary>
    public bool IsExactlyOneOf<T>()
        where T : IComponent
    {
        return IsExactlyOneOf(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the given component is one of the components which exactly one of must be on the entity
    /// </summary>
    public bool IsExactlyOneOf(Type type)
    {
        return IsExactlyOneOf(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the given component is one of the components which exactly one of must be on the entity
    /// </summary>
    public bool IsExactlyOneOf(ComponentId id)
    {
        return ExactlyOneOf.Contains(id);
    }

    #endregion

    #region Archetype Matching

    /// <summary>
    /// Get all archetypes which match this query
    /// </summary>
    public IReadOnlyList<Archetype> GetArchetypes()
    {
        return GetArchetypeMatchResult().Archetypes;
    }

    /// <summary>
    /// Get all archetypes which match this query
    /// </summary>
    public ImmutableOrderedListSet<ArchetypeMatch> GetArchetypeMatches()
    {
        return GetArchetypeMatchResult().ArchetypesMatches;
    }

    private ArchetypeMatchResult GetArchetypeMatchResult()
    {
        // Quick check if we already have a non-stale result
        resultLock.EnterReadLock();
        try
        {
            if (result != null && !result.Value.IsStale(World))
            {
                return result.Value;
            }
        }
        finally
        {
            resultLock.ExitReadLock();
        }

        // We don't have a valid cached result, calculate it now
        resultLock.EnterWriteLock();
        try
        {
            // If this query has never been evaluated before do it now
            if (result == null)
            {
                // Check every archetype
                var matches = new List<ArchetypeMatch>();
                foreach (var item in World.Archetypes)
                {
                    if (TryMatch(item) is ArchetypeMatch m)
                    {
                        matches.Add(m);
                    }
                }

                // Store result for next time
                result = new ArchetypeMatchResult(World.Archetypes.Count, ImmutableOrderedListSet<ArchetypeMatch>.Create(matches));

                // Return matches
                return result.Value;
            }

            // If the number of archetypes has changed since last time regenerate the cache
            if (result.Value.IsStale(World))
            {
                // Lazy copy of the match set, in case there are no matches
                var copy = default(OrderedListSet<ArchetypeMatch>?);

                // Check every new archetype
                for (var i = result.Value.ArchetypeWatermark; i < World.Archetypes.Count; i++)
                {
                    var m = TryMatch(World.Archetypes[i]);
                    if (m == null)
                    {
                        continue;
                    }

                    // Lazy copy the set now that we know we need it
                    copy ??= new OrderedListSet<ArchetypeMatch>(result.Value.ArchetypesMatches);

                    // Add the match
                    copy.Add(m.Value);
                }

                if (copy == null)
                {
                    // Copy is null, that means nothing new was found, just use the old result with the new watermark
                    result = new ArchetypeMatchResult(World.Archetypes.Count, result.Value.ArchetypesMatches);
                }
                else
                {
                    // Create a new match result
                    result = new ArchetypeMatchResult(World.Archetypes.Count, ImmutableOrderedListSet<ArchetypeMatch>.Create(copy));
                }
            }

            return result.Value;
        }
        finally
        {
            resultLock.ExitWriteLock();
        }
    }

    private ArchetypeMatch? TryMatch(Archetype archetype)
    {
        // Quick bloom filter test if the included components intersects with the archetype.
        // If this returns false there is definitely no overlap at all and we can early exit.
        if (Include.Count > 0 && !archetype.ComponentsBloomFilter.MaybeIntersects(in includeBloom))
        {
            return null;
        }

        // Do the full set check for included components
        if (!archetype.Components.IsSupersetOf(Include))
        {
            return null;
        }

        // If this is false it means there is definitely _not_ an intersection, which means we can skip
        // the inner check.
        if (Exclude.Count > 0 && excludeBloom.MaybeIntersects(in archetype.ComponentsBloomFilter))
        {
            if (archetype.Components.Overlaps(Exclude))
            {
                return null;
            }
        }

        // Use the temp hashset to do this
        var set = temporarySet;
        set.Clear();

        // Check if there are any "exactly one" items
        var exactlyOne = default(ComponentId?);
        if (ExactlyOneOf.Count > 0)
        {
            set.Clear();
            set.UnionWith(archetype.Components);
            set.IntersectWith(ExactlyOneOf);
            if (set.Count != 1)
            {
                set.Clear();
                return null;
            }

            exactlyOne = set.Single();
            set.Clear();
        }

        // Check if there are any "at least one" items
        if (AtLeastOneOf.Count > 0)
        {
            set.Clear();
            set.UnionWith(archetype.Components);
            set.IntersectWith(AtLeastOneOf);
            if (set.Count == 0)
            {
                set.Clear();
                return null;
            }
        }
        else
        {
            set.Clear();
            set = null;
        }

        var atLeastOne = set?.ToImmutable();

        return new ArchetypeMatch(archetype, atLeastOne, exactlyOne);
    }

    private readonly struct ArchetypeMatchResult
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
        /// The number of archetypes in the world when this cache was created. Used for caching purposes.
        /// </summary>
        public int ArchetypeWatermark { get; }

        public ArchetypeMatchResult(int watermark, ImmutableOrderedListSet<ArchetypeMatch> archetypesMatches)
        {
            ArchetypesMatches = archetypesMatches;
            ArchetypeWatermark = watermark;

            var archetypes = new List<Archetype>(archetypesMatches.Count);
            foreach (var match in archetypesMatches)
            {
                archetypes.Add(match.Archetype);
            }

            Archetypes = archetypes;
        }

        public bool IsStale(EcsWorld world)
        {
            return ArchetypeWatermark < world.ArchetypesCount;
        }
    }

    /// <summary>
    /// An archetype which matches a query
    /// </summary>
    /// <param name="Archetype">The archetype</param>
    /// <param name="AtLeastOne">All of the "at least one" components present (if there are any in this query)</param>
    /// <param name="ExactlyOne">The "exactly one" component present (if there is one in this query)</param>
    public readonly record struct ArchetypeMatch(Archetype Archetype, ImmutableOrderedListSet<ComponentId>? AtLeastOne, ComponentId? ExactlyOne) : IComparable<ArchetypeMatch>
    {
        /// <inheritdoc/>
        public int CompareTo(ArchetypeMatch other)
        {
            return Archetype.Hash.CompareTo(other.Archetype.Hash);
        }
    }

    #endregion

    #region LINQ

    /// <summary>
    /// Count how many entities match this query
    /// </summary>
    public int Count()
    {
        var count = 0;
        foreach (var archetype in GetArchetypes())
        {
            count += archetype.EntityCount;
        }

        return count;
    }

    /// <summary>
    /// Check if this query matches any entities
    /// </summary>
    public bool Any()
    {
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if this query description matches the given entity
    /// </summary>
    public bool Contains(Entity entity)
    {
        var location = entity.World.GetStorageLocation(entity.EntityId);
        var archetype = new ArchetypeMatch(location.Chunk.Archetype, null, null);
        return GetArchetypeMatches().Contains(archetype);
    }

    /// <summary>
    /// Get the first entity which this query matches (or null)
    /// </summary>
    public Entity? FirstOrDefault()
    {
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.EntityCount > 0)
                {
                    return chunk.Entities[0];
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Get the first entity which this query matches (or throw if there are none)
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if there are no matches</exception>
    public Entity First()
    {
        return FirstOrDefault()
            ?? throw new InvalidOperationException("QueryDescription.First() found no matching entities");
    }

    /// <summary>
    /// Get the single entity which this query matches (or null if there are none).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if there are more than one matches</exception>
    public Entity? SingleOrDefault()
    {
        Entity? result = null;
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.EntityCount == 0)
                {
                    continue;
                }

                if (chunk.EntityCount > 1 || result.HasValue)
                {
                    throw new InvalidOperationException("QueryDescription.SingleOrDefault() found more than one matching entity");
                }

                result = chunk.Entities[0];
            }
        }

        return result;
    }

    /// <summary>
    /// Get a single entity which this query matches (throws if there is not exactly one match)
    /// </summary>
    /// <exception cref="InvalidOperationException">If none or multiple entities were found.</exception>
    public Entity Single()
    {
        return SingleOrDefault()
            ?? throw new InvalidOperationException("QueryDescription.SingleOrDefault() found no matching entities");
    }

    /// <summary>
    /// Get a random entity matched by this query (or null if there are none).
    /// </summary>
    public Entity? RandomOrDefault(Random random)
    {
        // Get total entity count
        var count = Count();
        if (count == 0)
        {
            return default;
        }

        // Choose the index of the entity
        var choice = random.Next(0, count);

        // Find that entity
        foreach (var archetype in GetArchetypes())
        {
            // Check if it's within this archetype, if not move to the next archetype
            if (choice - archetype.EntityCount >= 0)
            {
                choice -= archetype.EntityCount;
            }
            else
            {
                // Step through chunks
                foreach (var chunk in archetype.Chunks)
                {
                    // Check if it's within this chunk, if not move to next chunk
                    if (choice - chunk.EntityCount >= 0)
                    {
                        choice -= chunk.EntityCount;
                    }
                    else
                    {
                        return chunk.Entities[choice];
                    }
                }
            }
        }

        // This shouldn't happen
        return default;
    }

    #endregion
}
