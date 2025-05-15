using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Worlds;

/// <remarks>
/// This is very similar to <see cref="StorageLocation"/>. Difference is that this also stores the numerical entity ID, not just the version.
/// </remarks>
internal readonly record struct EntityStorageLocation
{
    /// <summary>
    /// The current version and ID of this entity.
    /// </summary>
    public EntityId Entity { get; }

    /// <summary>
    /// The chunk in the archetype which contains this entity.
    /// </summary>
    public Chunk Chunk { get; }

    /// <summary>
    /// The entity index in the chunk which contains this entity.
    /// </summary>
    public int IndexInChunk { get; }

    internal EntityStorageLocation(EntityId entity, int indexInChunk, Chunk chunk)
    {
        Entity = entity;
        IndexInChunk = indexInChunk;
        Chunk = chunk;
    }

    public ref T GetMutable<T>() where T : IComponent
    {
        return ref Chunk.Get<T>(Entity, IndexInChunk);
    }
}
