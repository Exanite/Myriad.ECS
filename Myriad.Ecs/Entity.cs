using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// An <see cref="Entity"/> is an ID in the <see cref="World"/> which has a set of components associated with it.
/// </summary>
[DebuggerDisplay("{EntityId}")]
public readonly partial record struct Entity : IComparable<Entity>
{
    /// <summary>
    /// Check if this Entity still exists.
    /// </summary>
    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id != 0 && World.GetVersion(Id) == Version;
    }

    /// <summary>
    /// The <see cref="World"/> this <see cref="Entity"/> is in.
    /// </summary>
    public readonly EcsWorld World;

    /// <summary>
    /// The <see cref="Ecs.Entity"/> of an entity, may be re-used very quickly once an <see cref="Ecs.Entity"/> is destroyed.
    /// </summary>
    public int Id => EntityId.Id;

    /// <summary>
    /// The version number of this ID, may also be re-used but only after the full 32 bit counter has been overflowed for this specific ID.
    /// </summary>
    public uint Version => EntityId.Version;

    /// <summary>
    /// The raw ID of this <see cref="Entity"/>
    /// </summary>
    internal readonly EntityId EntityId;

    /// <summary>
    /// Get the set of components which this entity currently has.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ComponentIds
    {
        get
        {
            var location = World.GetStorageLocation(EntityId);
            return location.Chunk.Archetype.Components;
        }
    }

    /// <summary>
    /// Get a boxed array of all components.
    /// <para/>
    /// This is very slow and the returned data is a copy of the original data.
    /// Avoid using this for anything other than debugging!
    /// </summary>
    public object[] BoxedComponents => ComponentIds.Select(GetBoxedComponent).ToArray()!;

    /// <summary>
    /// Check if this entity has a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>() where T : IComponent
    {
        return ComponentIds.Contains(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>() where T : IComponent
    {
        ref var entityInfo = ref World.GetStorageLocation(EntityId);
        return ref entityInfo.Chunk.Get<T>(EntityId, entityInfo.IndexInChunk);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRef<T> GetComponentRef<T>() where T : IComponent
    {
        ref var entityInfo = ref World.GetStorageLocation(EntityId);
        return entityInfo.Chunk.GetRef<T>(EntityId, entityInfo.IndexInChunk);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentRef<T> GetStorableComponentRef<T>() where T : IComponent
    {
        return new ComponentRef<T>(this);
    }

    /// <summary>
    /// Get a <b>boxed copy</b> of a component from this entity. Only use for debugging!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetBoxedComponent(ComponentId id)
    {
        if (!IsAlive)
        {
            return null;
        }

        if (!ComponentIds.Contains(id))
        {
            return null;
        }

        ref var entityInfo = ref World.GetStorageLocation(EntityId);
        return entityInfo.Chunk.GetComponentArray(id).GetValue(entityInfo.IndexInChunk);
    }

    internal Entity(EntityId id, EcsWorld world)
    {
        EntityId = id;
        World = world;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return EntityId.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Entity other)
    {
        return EntityId.CompareTo(other.EntityId);
    }
}
