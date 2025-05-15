using System;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// An entity that has been created in a command buffer, but not yet created. Can be used to add components.
/// </summary>
public readonly record struct BufferedEntity
{
    private readonly uint id;
    private readonly uint version;

    private readonly EcsCommandBuffer commandBuffer;
    private readonly EcsCommandBufferResolver resolver;

    /// <summary>
    /// Get the <see cref="EcsCommandBuffer"/> which this <see cref="BufferedEntity"/> is from.
    /// </summary>
    public EcsCommandBuffer CommandBuffer
    {
        get
        {
            EnsureIsMutable();
            return commandBuffer;
        }
    }

    internal BufferedEntity(uint id, EcsCommandBuffer commandBuffer, EcsCommandBufferResolver resolver)
    {
        this.id = id;
        this.commandBuffer = commandBuffer;
        this.resolver = resolver;

        version = commandBuffer.Version;
    }

    /// <summary>
    /// Add or overwrite a component attached to this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="value">The value of the component to add.</param>
    /// <returns>This buffered entity.</returns>
    public BufferedEntity Set<T>(T value) where T : IComponent
    {
        EnsureIsMutable();

        commandBuffer.SetBuffered(id, value);
        return this;
    }

    /// <summary>
    /// Resolve this <see cref="BufferedEntity"/> into the real <see cref="Entity"/> that was constructed.
    /// </summary>
    public Entity Resolve()
    {
        if (resolver.Parent == null)
        {
            throw new ObjectDisposedException("Resolver has already been disposed");
        }

        if (resolver.Parent != commandBuffer)
        {
            throw new InvalidOperationException("Cannot use a resolver from one command buffer with buffered entity from another");
        }

        if (resolver.Version != version)
        {
            throw new ObjectDisposedException("Resolver has already been disposed");
        }

        return resolver.Lookup[id].ToEntity(resolver.World);
    }

    private void EnsureIsMutable()
    {
        if (version != commandBuffer.Version)
        {
            throw new InvalidOperationException("Cannot use buffered entity after command buffer has been executed");
        }
    }
}
