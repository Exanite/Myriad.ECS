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
    public readonly int WorldId;

    internal EntityManager Entities = new();

    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    internal readonly Dictionary<QueryCacheKey, QueryView> QueryViewCache = new();
    private readonly QueryView allEntitiesQuery;

    private readonly Pool<EcsCommandBuffer> commandBufferPool;
    private readonly HashSet<EcsCommandBuffer> activeCommandBuffers = new();

    /// <summary>
    /// The archetypes stored by this world.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => archetypes;

    public EventBus EventBus { get; } = new();

    public EcsWorld()
    {
        using (IdLock.EnterScope())
        {
            WorldId = NextWorldId++;
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
    public IEntityLookup AddTo(EcsWorld dstWorld, IArchetypeView view)
    {
        using var _ = AcquireCommandBuffer(out var commandBuffer);
        using var __ = ListPool<Chunk>.Acquire(out var newChunks);

        var lookup = new EntityLookup();
        foreach (var srcArchetype in view.Archetypes)
        {
            if (srcArchetype.EntityCount == 0)
            {
                continue;
            }

            var dstArchetype = dstWorld.GetOrCreateArchetype(srcArchetype.Components.AsComponentIdSet(), srcArchetype.Hash);
            foreach (var srcChunk in srcArchetype.Chunks)
            {
                var newChunk = dstArchetype.CreateChunkFrom(srcChunk, commandBuffer, lookup);
                newChunks.Add(newChunk);
            }
        }

        foreach (var dstChunk in newChunks)
        {
            var componentIdByColumnIndex = dstChunk.Lookup.ComponentIdByColumnIndex;
            foreach (var componentId in componentIdByColumnIndex)
            {
                var dispatcher = dstChunk.Lookup.ComponentDispatcherByComponentId[componentId.Value];
                dispatcher.OnComponentCopied(commandBuffer, dstChunk, lookup);
                dispatcher.OnComponentAdded(commandBuffer, dstChunk);
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
        var a = new Archetype(this, components.ToImmutableOrderedListSet());

        // Add it to the relevant lists
        archetypes.Add(a);
        candidates.Add(a);

        return a;
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
