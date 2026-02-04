namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// An entity that is being processed by a command buffer.
/// Mainly used for fluent method chaining.
/// </summary>
public readonly ref struct BufferedEntity
{
    public readonly Entity Entity;
    public readonly EcsCommandBuffer CommandBuffer;

    internal BufferedEntity(Entity entity, EcsCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;
    }

    public static implicit operator Entity(BufferedEntity value)
    {
        return value.Entity;
    }

    /// <inheritdoc cref="EcsCommandBuffer.Set"/>
    public BufferedEntity Set<T>(T value) where T : IComponent
    {
        return CommandBuffer.Set(Entity, value);
    }

    /// <inheritdoc cref="EcsCommandBuffer.Set"/>
    public BufferedEntity SetBoxed(object value)
    {
        return CommandBuffer.SetBoxed(Entity, value);
    }

    /// <inheritdoc cref="EcsCommandBuffer.Remove"/>
    public BufferedEntity Remove<T>() where T : IComponent
    {
        return CommandBuffer.Remove<T>(Entity);
    }

    /// <inheritdoc cref="EcsCommandBuffer.CopyFromInternal"/>
    public BufferedEntity CopyFrom(Entity prefab)
    {
        return CommandBuffer.CopyFrom(Entity, prefab);
    }

    /// <inheritdoc cref="EcsCommandBuffer.CopyFromInternal"/>
    public BufferedEntity CopyFrom(Entity prefab, Entity groupKey)
    {
        return CommandBuffer.CopyFrom(Entity, prefab, groupKey);
    }
}
