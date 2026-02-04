using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after a component is copied to a new world.
/// </summary>
public readonly ref struct ComponentCopiedEvent<T> where T : IComponent
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
    public readonly IEntityLookup Lookup;

    public ComponentCopiedEvent(EcsCommandBuffer commandBuffer, Entity entity, ref T component, IEntityLookup lookup)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        Component = ref component;
        Lookup = lookup;
    }
}
