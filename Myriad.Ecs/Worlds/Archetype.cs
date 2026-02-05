using System;
using System.Collections.Generic;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// An archetype contains all entities which share exactly the same set of components.
/// </summary>
public sealed class Archetype
{
    /// <summary>
    /// The world which this archetype belongs to.
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The total number of entities in this archetype.
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// The chunks contained in this archetype.
    /// </summary>
    public ReadOnlySpan<Chunk> Chunks => chunksList.AsSpan();

    /// <summary>
    /// The chunks contained in this archetype.
    /// </summary>
    /// <remarks>
    /// Enumerating over this will allocate due to the List enumerator being boxed.
    /// </remarks>
    public IReadOnlyList<Chunk> ChunksList => chunksList;

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Components { get; }

    /// <inheritdoc cref="ArchetypeComponentLookup"/>
    internal readonly ArchetypeComponentLookup Lookup;

    /// <summary>
    /// A bloom filter of all the components in this archetype.
    /// </summary>
    internal readonly ComponentBloomFilter ComponentsBloomFilter;

    /// <summary>
    /// The hash of all components IDs in this archetype.
    /// </summary>
    internal ArchetypeHash Hash { get; }

    /// <summary>
    /// All chunks in this archetype.
    /// </summary>
    private readonly List<Chunk> chunksList = [];

    /// <summary>
    /// A list of chunks which might have space to put an entity in.
    /// </summary>
    private readonly List<Chunk> chunksWithSpace = [];

    /// <summary>
    /// A list of empty chunks that have been removed from this archetype.
    /// </summary>
    private readonly Stack<Chunk> spareChunks = new(EcsConstants.ChunkHotSpareCount);

    internal Archetype(EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        World = world;
        Components = components;
        ComponentsBloomFilter = components.ToBloomFilter();

        // Calculate archetype hash
        foreach (var component in components)
        {
            Hash = Hash.Toggle(component);
        }

        // Initialize component lookup
        Lookup = new ArchetypeComponentLookup(components);
    }

    /// <summary>
    /// Destroy every Entity in this archetype
    /// </summary>
    internal void Clear()
    {
        // Clear all the chunks
        foreach (var chunk in chunksList)
        {
            chunk.Clear();
        }

        // Move some chunks to hot spares and then destroy the rest
        foreach (var chunk in chunksList)
        {
            if (spareChunks.Count < EcsConstants.ChunkHotSpareCount)
            {
                spareChunks.Push(chunk);
            }
            else
            {
                break;
            }
        }
        chunksWithSpace.Clear();
        chunksList.Clear();

        // Done! No entities left.
        EntityCount = 0;
    }

    /// <summary>
    /// Copies the entities from the source chunk to a new chunk in this archetype.
    /// The source chunk must have the same component set.
    /// </summary>
    /// <remarks>
    /// This is designed to be called by <see cref="EcsWorld.AddTo(EcsWorld, IArchetypeView)"/>.
    /// </remarks>
    internal Chunk CreateChunkFrom(Chunk srcChunk, EcsCommandBuffer recursiveCommandBuffer, EntityLookup lookup)
    {
        EntityCount += srcChunk.Entities.Length;

        var newChunk = GetEmptyChunk();
        newChunk.CopyFrom(srcChunk, recursiveCommandBuffer, lookup);

        return newChunk;
    }

    /// <summary>
    /// Find a chunk with space and add the given entity to it.
    /// </summary>
    /// <param name="entity">Entity to add to a chunk</param>
    /// <param name="location">Location will be mutated to point to the new location</param>
    internal void AddEntity(EntityId entity, ref EntityLocation location)
    {
        EntityCount++;

        var chunk = GetChunkWithSpace();
        chunk.AddEntity(entity, ref location);
    }

    /// <summary>
    /// Returns a chunk that is not full.
    /// </summary>
    private Chunk GetChunkWithSpace()
    {
        chunksWithSpace.RemoveAll(static chunk => chunk.IsFull);
        if (chunksWithSpace.Count > 0)
        {
            return chunksWithSpace[0];
        }

        return GetEmptyChunk();
    }

    /// <summary>
    /// Returns a chunk that is completely empty.
    /// </summary>
    private Chunk GetEmptyChunk()
    {
        var newChunk = spareChunks.Count > 0 ? spareChunks.Pop() : new Chunk(this);
        chunksList.Add(newChunk);
        chunksWithSpace.Add(newChunk);

        return newChunk;
    }

    internal void RemoveEntity(EntityLocation location)
    {
        // Remove the entity from the chunk, component data is lost after this point
        location.Chunk.RemoveEntity(location);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved(location.Chunk);
    }

    internal void MigrateEntity(EntityId entity, Archetype dstArchetype, ref EntityLocation location)
    {
        GuardUtility.IsFalse(dstArchetype == this, "Destination archetype is the same as the source archetype");

        // Do the actual copying
        var srcChunk = location.Chunk;
        srcChunk.MigrateTo(entity, ref location, dstArchetype);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved(srcChunk);
    }

    private void OnChunkEntityRemoved(Chunk chunk)
    {
        // Decrease archetype entity count
        EntityCount--;

        switch (chunk.Entities.Length)
        {
            // If the chunk is empty remove it from this archetype entirely
            case 0:
            {
                chunksWithSpace.Remove(chunk);
                chunksList.Remove(chunk);
                if (spareChunks.Count < EcsConstants.ChunkHotSpareCount)
                {
                    spareChunks.Push(chunk);
                }

                break;
            }

            // If the chunk was previously full and now isn't, add it to the set of chunks with space
            case EcsConstants.ChunkEntityCount - 1:
            {
                chunksWithSpace.Add(chunk);
                break;
            }
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Hash.GetHashCode();
    }
}
