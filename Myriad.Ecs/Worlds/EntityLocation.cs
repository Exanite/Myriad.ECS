namespace Exanite.Myriad.Ecs.Worlds;

internal struct EntityLocation
{
    /// <summary>
    /// The current version of this entity.
    /// </summary>
    public uint Version;

    /// <summary>
    /// The chunk that contains this entity.
    /// </summary>
    public Chunk Chunk;

    /// <summary>
    /// The entity index in the chunk which contains this entity.
    /// </summary>
    public int IndexInChunk;
}
