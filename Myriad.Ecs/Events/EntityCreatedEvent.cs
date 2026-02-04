using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after an entity is added to the world.
/// Specifically, after its initial set of components are added.
/// </summary>
public readonly ref struct EntityCreatedEvent
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

    public EntityCreatedEvent(EcsCommandBuffer commandBuffer, Entity entity)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
    }
}
