using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised before an entity is removed from the world.
/// Specifically, before its components are removed.
/// </summary>
public readonly ref struct EntityDestroyedEvent
{
    /// <summary>
    /// A command buffer with which further operations can be enqueued.
    /// This command buffer will run after the command buffer that raised this event has completed.
    /// </summary>
    /// <remarks>
    /// Not the command buffer that raised this event.
    /// </remarks>
    public readonly EcsCommandBuffer CommandBuffer;

    public EcsWorld World => Entity.World;
    public readonly Entity Entity;

    public EntityDestroyedEvent(EcsCommandBuffer commandBuffer, Entity entity)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
    }
}
