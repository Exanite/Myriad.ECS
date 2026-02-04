using System;
using System.Runtime.CompilerServices;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// The ID of an <see cref="Entity"/> (not carrying a reference to a <see cref="EcsWorld"/>)
/// </summary>
internal readonly record struct EntityId : IComparable<EntityId>
{
    /// <inheritdoc cref="Entity.Index"/>
    public readonly int Index;

    /// <inheritdoc cref="Entity.Version"/>
    public readonly uint Version;

    internal EntityId(int index, uint version)
    {
        Index = index;
        Version = version;
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> struct based on this <see cref="EntityId"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ToEntity(EcsWorld world)
    {
        return new Entity(this, world);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"{Index}:{Version}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(EntityId other)
    {
        var order = Index.CompareTo(other.Index);
        if (order != 0)
        {
            return order;
        }

        return Version.CompareTo(other.Version);
    }
}
