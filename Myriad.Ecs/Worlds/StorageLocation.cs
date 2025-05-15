using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Worlds;

internal struct StorageLocation
{
    /// <summary>
    /// The current version of this entity.
    /// </summary>
    public uint Version;

    /// <summary>
    /// The chunk in the archetype which contains this entity.
    /// </summary>
    public Chunk Chunk;

    /// <summary>
    /// The entity index in the chunk which contains this entity.
    /// </summary>
    public int IndexInChunk;

    public readonly EntityStorageLocation GetEntityStorageLocation(EntityId entity)
    {
        return new EntityStorageLocation(entity, IndexInChunk, Chunk);
    }
}
