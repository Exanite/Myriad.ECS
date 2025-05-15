using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after an entity is created and added to the world.
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

/// <summary>
/// Raised before an entity is destroyed and removed from the world.
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

/// <summary>
/// Raised after a component is either added or set.
/// </summary>
public readonly ref struct ComponentAdded<T> where T : IComponent
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
    public readonly ref T Value;

    public ComponentAdded(EcsCommandBuffer commandBuffer, Entity entity, ref T value)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        Value = ref value;
    }
}

/// <summary>
/// Raised after an existing component is explicitly set.
/// <br/>
/// Warning: Modifications without setting through the command buffer will NOT raise this event.
/// </summary>
/// <remarks>
/// This event does not provide the old value because the event cannot guarantee that
/// the component has not been modified since the previous <see cref="ComponentModified{T}"/> event.
/// <para/>
/// Code that relies on the previous component value should store it manually.
/// </remarks>
public readonly ref struct ComponentModified<T> where T : IComponent
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
    public readonly ref T Value;

    public ComponentModified(EcsCommandBuffer commandBuffer, Entity entity, ref T value)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        Value = ref value;
    }
}

/// <summary>
/// Raised before a component is either removed.
/// </summary>
public readonly ref struct ComponentRemoved<T> where T : IComponent
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
    public readonly ref T Value;

    public ComponentRemoved(EcsCommandBuffer commandBuffer, Entity entity, ref T value)
    {
        CommandBuffer = commandBuffer;
        Entity = entity;
        Value = ref value;
    }
}
