namespace Exanite.Myriad.Ecs;

internal static class EcsConstants
{
    /// <summary>
    /// The size of each segment in the storage location lookup list.
    /// </summary>
    internal const int StorageLocationSegmentSize = 4096;

    /// <summary>
    /// The initial entity capacity of an archetype.
    /// </summary>
    internal const int ArchetypeInitialCapacity = 1024;

    /// <summary>
    /// Number of local entity IDs to keep per command buffer for the purpose of avoiding thread contention.
    /// </summary>
    internal const int CommandBufferLocalIdCount = 32;
}
