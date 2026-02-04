using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after an existing component is explicitly set.
/// <br/>
/// Warning: Modifications without setting through the command buffer will NOT raise this event.
/// </summary>
/// <remarks>
/// This event does not provide the old value because the event cannot guarantee that
/// the component has not been modified since the previous <see cref="ComponentModifiedEvent{T}"/> event.
/// <para/>
/// Code that relies on the previous component value should store it manually.
/// </remarks>
public readonly ref struct ComponentModifiedEvent<T> where T : IComponent
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
    public readonly ref T Component;

    public ComponentModifiedEvent(EcsCommandBuffer commandBuffer, Entity entity, ref T component)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        Component = ref component;
    }
}
