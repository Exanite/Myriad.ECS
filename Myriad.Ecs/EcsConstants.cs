namespace Exanite.Myriad.Ecs;

internal static class EcsConstants
{
    /// <summary>
    /// The size of each segment in the storage location lookup list.
    /// </summary>
    internal const int StorageLocationSegmentSize = 1024;

    /// <summary>
    /// Number of local entity IDs to keep per command buffer for the purpose of avoiding thread contention.
    /// </summary>
    internal const int CommandBufferLocalIdCount = 32;

    /// <summary>
    /// Number of entities in a single chunk.
    /// </summary>
    internal const int ChunkEntityCount = 1024;

    /// <summary>
    /// How many empty chunks to keep as spares.
    /// </summary>
    internal const int ChunkHotSpareCount = 4;

    /// <summary>
    /// The max number of edges used by the command buffer for calculating entity archetypes.
    /// </summary>
    internal const int MaxAggregationEdges = 1024;
}
