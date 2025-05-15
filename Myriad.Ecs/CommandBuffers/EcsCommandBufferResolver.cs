using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Provides a way to resolve created entities. Must be disposed once finished with!
/// </summary>
public sealed class EcsCommandBufferResolver : IDisposable
{
    internal SortedList<uint, EntityId> Lookup { get; } = [];
    internal EcsCommandBuffer? Parent { get; private set; }

    internal uint Version { get; private set; }

    /// <summary>
    /// Get the number of entities in this <see cref="EcsCommandBufferResolver"/>
    /// </summary>
    public int Count => Lookup.Count;

    /// <summary>
    /// The <see cref="World"/> this resolver is for.
    /// </summary>
    public EcsWorld World => Parent!.World;

    internal void Configure(EcsCommandBuffer buffer)
    {
        Lookup.Clear();
        Parent = buffer;
        Version = buffer.Version;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Parent == null)
        {
            throw new ObjectDisposedException(nameof(EcsCommandBufferResolver));
        }

        unchecked
        {
            Version--;
        }

        Parent = null;
        Lookup.Clear();

        SimplePool.Release(this);
    }

    /// <summary>
    /// Get the nth item in this <see cref="EcsCommandBufferResolver"/>. Items are in an arbitrary order.
    /// </summary>
    public Entity this[int index] => Lookup.Values[index].ToEntity(World);
}
