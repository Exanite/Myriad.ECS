﻿using System.Collections.Concurrent;
using Myriad.ECS.Collections;
using Myriad.ECS.Command;
using Myriad.ECS.IDs;
using Myriad.ECS.Queries;
using Myriad.ECS.Threading;
using Myriad.ECS.Worlds.Archetypes;

namespace Myriad.ECS.Worlds;

public sealed partial class World
    : IDisposable
{
    internal IThreadPool ThreadPool { get; }

    private readonly List<Archetype> _archetypes = [ ];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> _archetypesByHash = [ ];

    // Keep track of dead entities so their ID can be re-used
    private readonly List<Entity> _deadEntities = [ ];
    private int _nextEntityId = 1;

    private readonly SegmentedList<EntityInfo> _entities = new(1024);

    public IReadOnlyList<Archetype> Archetypes => _archetypes;
    internal int ArchetypesCount => _archetypes.Count;

    private readonly ConcurrentBag<CommandBuffer> _commandBufferPool = [ ];

    internal World(IThreadPool pool)
    {
        ThreadPool = pool;
    }

    public void Dispose()
    {
        var lazy = new LazyCommandBuffer(this);

        foreach (var archetype in _archetypes)
            archetype.Dispose(ref lazy);

        if (lazy.TryGetBuffer(out var buffer))
            buffer.Playback().Dispose();
    }

    internal CommandBuffer GetPooledCommandBuffer()
    {
        if (!_commandBufferPool.TryTake(out var buffer))
            buffer = new CommandBuffer(this);
        return buffer;
    }

    internal void ReturnPooledCommandBuffer(CommandBuffer buffer)
    {
        if (_commandBufferPool.Count < 32)
            _commandBufferPool.Add(buffer);
    }

    #region bulk write
    /// <summary>
    /// Overwrite the value of a specific component on every entity which matches the given query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public int Overwrite<T>(T item, QueryDescription? query = null)
        where T : IComponent
    {
        return Overwrite(item, ref query);
    }

    /// <summary>
    /// Overwrite the value of a specific component on every entity which matches the given query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public int Overwrite<T>(T item, ref QueryDescription? query)
        where T : IComponent
    {
        query ??= GetCachedQuery<T>();
        return query.Overwrite(item);
    }
    #endregion

    internal void DeleteImmediate(Entity delete, ref LazyCommandBuffer lazy)
    {
        // Get the entityinfo for this entity
        ref var entityInfo = ref _entities[delete.ID];

        // Check this is still a valid entity reference. Early exit if the entity
        // is already dead.
        if (entityInfo.Version != delete.Version)
            return;

        // Increment version, this will invalid the handle
        entityInfo.Version++;

        // Notify archetype this entity is dead
        entityInfo.Chunk.Archetype.RemoveEntity(entityInfo, ref lazy);

        // Store this ID for re-use later
        _deadEntities.Add(delete);
    }

    internal Archetype GetArchetype(Entity entity)
    {
        if (entity.ID < 0 || entity.ID >= _entities.TotalCapacity)
            throw new ArgumentException("Invalid entity ID", nameof(entity));

        return GetEntityInfo(entity).Chunk.Archetype;
    }

    /// <summary>
    /// Get the current version for a given entity ID
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns>The entity ID, or zero if the entity does not exist</returns>
    internal uint GetVersion(int entityId)
    {
        if (entityId <= 0 || entityId >= _entities.TotalCapacity)
            return 0;
        return _entities[entityId].Version;
    }

    /// <summary>
    /// Find an archetype with the given set of components, using a precomputed archetype hash.
    /// </summary>
    /// <param name="components"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentID> components, ArchetypeHash hash)
    {
        // Get list of all archetypes with this hash
        if (!_archetypesByHash.TryGetValue(hash, out var candidates))
        {
            candidates = [ ];
            _archetypesByHash.Add(hash, candidates);
        }

        // Check if any of the candidates are the one we need
        foreach (var archetype in candidates)
            if (archetype.SetEquals(components))
                return archetype;

        // Didn't find one, create the new archetype
        var a = new Archetype(this, new FrozenOrderedListSet<ComponentID>(components));

        // Add it to the relevant lists
        _archetypes.Add(a);
        candidates.Add(a);

        return a;
    }

    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentID> components)
    {
        // Build archetype hash to accelerate querying
        var hash = ArchetypeHash.Create(components);

        return GetOrCreateArchetype(components, hash);
    }

    internal Row MigrateEntity(Entity entity, Archetype to, ref LazyCommandBuffer lazy)
    {
        ref var info = ref GetEntityInfo(entity);
        return info.Chunk.Archetype.MigrateTo(entity, ref info, to, ref lazy);
    }

    internal ref EntityInfo AllocateEntity(out Entity entity)
    {
        if (_deadEntities.Count > 0)
        {
            var prev = _deadEntities[^1];
            _deadEntities.RemoveAt(_deadEntities.Count - 1);

            var v = unchecked(prev.Version + 1);
            if (v == 0)
                v += 1;

            entity = new Entity(prev.ID, v);
        }
        else
        {
            // Allocate a new ID. This **must not** overflow!
            entity = new Entity(checked(_nextEntityId++), 1);

            // Check if the collection of all entities needs to grow
            if (entity.ID >= _entities.TotalCapacity)
                _entities.Grow();
        }

        // Update the version
        ref var slot = ref _entities[entity.ID];
        slot.Version = entity.Version;

        return ref slot;
    }

    internal Row GetRow(Entity entity)
    {
        var info = GetEntityInfo(entity);
        return new Row(entity, info.RowIndex, info.Chunk);
    }

    internal ref EntityInfo GetEntityInfo(Entity entity)
    {
        ref var info = ref _entities[entity.ID];

        if (info.Version != entity.Version)
            throw new ArgumentException("entity is not alive", nameof(entity));

        return ref info;
    }
}