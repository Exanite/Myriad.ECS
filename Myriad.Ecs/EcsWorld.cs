using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Events;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// A world contains all entities.
/// </summary>
public sealed class EcsWorld : IArchetypeView, ITrackedDisposable
{
    private static readonly Lock IdLock = new();
    private static int NextWorldId = 1;

    internal EntityManager Entities = new();

    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    /// <summary>
    /// Must be read using <see cref="Volatile"/>.
    /// </summary>
    /// <remarks>
    /// Guaranteed to be the same as the archetype count,
    /// however, since this is a field, it can be read using <see cref="Volatile"/>.
    /// </remarks>
    internal int Version;

    internal readonly Lock QueryViewCacheLock = new();
    internal readonly Dictionary<QueryCacheKey, QueryView> QueryViewCache = new();
    private readonly QueryView allEntitiesQuery;

    private readonly Pool<EcsCommandBuffer> commandBufferPool;
    private readonly HashSet<EcsCommandBuffer> activeCommandBuffers = new();

    private readonly Lock recycleLock = new();
    private readonly List<List<Archetype>> archetypeListsToRecycle = new();

    public bool IsDisposing { get; private set; }
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The unique identifier for this world.
    /// </summary>
    /// <remarks>
    /// If too many worlds have been created,
    /// this can overflow and lead to id collisions,
    /// but this is mainly for debugging and
    /// you have to create a lot of worlds to overflow.
    /// </remarks>
    public readonly int Id;

    /// <summary>
    /// The archetypes stored by this world.
    /// </summary>
    /// <remarks>
    /// This can include empty archetypes.
    /// </remarks>
    public ReadOnlySpan<Archetype> Archetypes => archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => archetypes;

    public EventBus EventBus { get; } = new();

    public EcsWorld()
    {
        using (IdLock.EnterScope())
        {
            Id = NextWorldId++;
        }

        allEntitiesQuery = new QueryFilter().Build(this);

        commandBufferPool = new Pool<EcsCommandBuffer>(
            create: () => new EcsCommandBuffer(this),
            onAcquire: commandBuffer =>
            {
                activeCommandBuffers.Add(commandBuffer);
            },
            onRelease: commandBuffer =>
            {
                commandBuffer.Clear();
                activeCommandBuffers.Remove(commandBuffer);
            });
    }

    public Pool<EcsCommandBuffer>.Handle AcquireCommandBuffer(out EcsCommandBuffer value)
    {
        return commandBufferPool.Acquire(out value);
    }

    public EcsCommandBuffer AcquireCommandBuffer()
    {
        return commandBufferPool.Acquire();
    }

    public void ReleaseCommandBuffer(EcsCommandBuffer value)
    {
        commandBufferPool.Release(value);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be overwritten.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup CopyTo(EcsWorld dstWorld)
    {
        return CopyTo(dstWorld, allEntitiesQuery);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world for the specified archetypes.
    /// Data in the target world will be overwritten.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup CopyTo(EcsWorld dstWorld, IArchetypeView view)
    {
        dstWorld.Clear();
        return AddTo(dstWorld, view);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be kept.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup AddTo(EcsWorld dstWorld)
    {
        return AddTo(dstWorld, allEntitiesQuery);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be kept.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup AddTo(EcsWorld dstWorld, IArchetypeView view)
    {
        OnSyncPoint();

        using var _ = AcquireCommandBuffer(out var commandBuffer);
        using var __ = ListPool<Archetype>.Acquire(out var dstArchetypes);

        var lookup = new EntityLookup();
        var srcArchetypes = view.Archetypes;

        foreach (var srcArchetype in srcArchetypes)
        {
            if (srcArchetype.Entities.Length == 0)
            {
                continue;
            }

            var dstArchetype = dstWorld.GetOrCreateArchetype(srcArchetype.Components.AsComponentIdSet(), srcArchetype.Info.Hash);
            dstArchetype.AddFrom(srcArchetype, lookup);
            dstArchetypes.Add(dstArchetype);
        }

        for (var archetypeI = 0; archetypeI < srcArchetypes.Length; archetypeI++)
        {
            var srcArchetype = srcArchetypes[archetypeI];
            var dstArchetype = dstArchetypes[archetypeI];

            // Raise component copied/added events
            var dstStart = dstArchetype.Entities.Length - srcArchetype.Entities.Length;
            var dstComponentIdByColumnIndex = dstArchetype.Info.ComponentIdByColumnIndex;
            foreach (var componentId in dstComponentIdByColumnIndex)
            {
                var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                dispatcher.OnComponentCopied(commandBuffer, dstArchetype, dstStart, srcArchetype.Entities.Length, lookup);
                dispatcher.OnComponentAdded(commandBuffer, dstArchetype, dstStart, srcArchetype.Entities.Length);
            }

            // Raise entity created events
            for (var entityI = 0; entityI < srcArchetype.Entities.Length; entityI++)
            {
                dstWorld.EventBus.Raise(new EntityCreatedEvent(commandBuffer, dstArchetype.Entities[dstStart + entityI]));
            }
        }

        commandBuffer.Execute();

        return lookup;
    }

    /// <summary>
    /// Clears the world by destroying all entities.
    /// </summary>
    public void Clear()
    {
        OnSyncPoint();

        using var _ = AcquireCommandBuffer(out var commandBuffer);
        commandBuffer.Destroy(allEntitiesQuery);
        commandBuffer.Execute();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposing = true;

        // Destroy all entities
        Clear();

        // Clear all active command buffers
        foreach (var activeCommandBuffer in activeCommandBuffers)
        {
            activeCommandBuffer.Clear();
        }
        activeCommandBuffers.Clear();

        // Clear event handlers
        EventBus.Dispose();

        IsDisposing = false;
        IsDisposed = true;

        GuardUtility.IsTrue(allEntitiesQuery.Count() == 0, "Expected entity count to be 0 after world disposal");
    }

    internal void Recycle(List<Archetype> value)
    {
        using var _ = recycleLock.EnterScope();
        archetypeListsToRecycle.Add(value);
    }

    /// <summary>
    /// Call when a sync point is reached to clean up internal data.
    /// </summary>
    internal void OnSyncPoint()
    {
        using var _ = recycleLock.EnterScope();

        foreach (var value in archetypeListsToRecycle)
        {
            ListPool<Archetype>.Release(value);
        }
        archetypeListsToRecycle.Clear();
    }

    /// <summary>
    /// Find an archetype with the given set of components, using a precomputed archetype hash.
    /// </summary>
    internal Archetype GetOrCreateArchetype<T>(T components, ArchetypeHash hash) where T : IComponentIdSet
    {
        // Get list of all archetypes with this hash
        if (!archetypesByHash.TryGetValue(hash, out var candidates))
        {
            candidates = [];
            archetypesByHash.Add(hash, candidates);
        }

        // Check if any of the candidates are the one we need
        foreach (var archetype in candidates)
        {
            if (components.SetEquals(archetype.Components))
            {
                return archetype;
            }
        }

        // Didn't find one, create the new archetype
        var newArchetype = new Archetype(archetypes.Count, this, components.ToImmutableOrderedListSet());

        // Add it to the relevant lists
        archetypes.Add(newArchetype);
        candidates.Add(newArchetype);

        // Increment version
        Interlocked.Increment(ref Version);

        return newArchetype;
    }

    /// <summary>
    /// Find an archetype with the given set of components.
    /// </summary>
    internal Archetype GetOrCreateArchetype<T>(T components) where T : IComponentIdSet
    {
        return GetOrCreateArchetype(components, components.CreateArchetypeHash());
    }

    /// <inheritdoc cref="GetOrCreateArchetype{T}(T)"/>
    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentId> components)
    {
        return GetOrCreateArchetype(components.AsComponentIdSet());
    }
}
