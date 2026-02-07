using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// An <see cref="Entity"/> is an ID in the <see cref="World"/> which has a set of components associated with it.
/// </summary>
public readonly partial record struct Entity : IComparable<Entity>
{
    /// <summary>
    /// Check if this entity still exists.
    /// </summary>
    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (Index == 0)
            {
                return false;
            }

            ref var location = ref World.Entities.GetLocation(Index);
            return location.Version == Version && location.Archetype != null!;
        }
    }

    /// <summary>
    /// Check if this entity is pending creation.
    /// </summary>
    public bool IsPending
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (Index == 0)
            {
                return false;
            }

            ref var location = ref World.Entities.GetLocation(Index);
            return location.Version == Version && location.Archetype == null!;
        }
    }

    /// <summary>
    /// Check if this entity is default initialized.
    /// </summary>
    internal bool IsDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Index == 0;
    }

    /// <summary>
    /// The <see cref="World"/> this <see cref="Entity"/> is in.
    /// </summary>
    public readonly EcsWorld World;

    /// <summary>
    /// The archetype of the entity.
    /// </summary>
    public Archetype Archetype => World.Entities.GetLocation(EntityId).Archetype;

    /// <summary>
    /// The index of this entity.
    /// May be re-used very quickly once an <see cref="Entity"/> is destroyed.
    /// </summary>
    public int Index => EntityId.Index;

    /// <summary>
    /// The version of this entity.
    /// May be re-used, but only after the full 32 bit counter has been overflowed for this specific index.
    /// </summary>
    public uint Version => EntityId.Version;

    /// <summary>
    /// The raw ID of this <see cref="Entity"/>.
    /// </summary>
    internal readonly EntityId EntityId;

    /// <summary>
    /// Get the set of components which this entity currently has.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ComponentIds => Archetype.Components;

    /// <summary>
    /// Get a boxed array of all components.
    /// <para/>
    /// This is very slow and the returned data is a copy of the original data.
    /// Avoid using this for anything other than debugging!
    /// </summary>
    public object[] BoxedComponents => ComponentIds.Select(GetBoxed).ToArray()!;

    internal Entity(EntityId id, EcsWorld world)
    {
        EntityId = id;
        World = world;
    }

    /// <summary>
    /// Check if this entity has a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : IComponent
    {
        return ComponentIds.Contains(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return ref location.Archetype.Get<T>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ref<T> GetRef<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Archetype.GetRef<T>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsRef<T> GetEcsRef<T>() where T : IComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity does not exist");
        GuardUtility.IsTrue(Has<T>(), $"Component does not exist on entity: {GetType().Name}");

        return new EcsRef<T>(this);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// No exception will be thrown if the entity does not have this component.
    /// </summary>
    /// <remarks>
    /// This is useful when the entity has pending command buffer changes.
    /// Accessing the component will still validate the reference.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsRef<T> GetEcsRefUnchecked<T>() where T : IComponent
    {
        return new EcsRef<T>(this);
    }

    /// <summary>
    /// Get a <b>boxed copy</b> of a component from this entity. Only use for debugging!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetBoxed(ComponentId id)
    {
        if (!IsAlive)
        {
            return null;
        }

        if (!ComponentIds.Contains(id))
        {
            return null;
        }

        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Archetype.GetComponentArray(id).GetValue(location.IndexInArchetype);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        if (World == null)
        {
            return "0:0:0";
        }

        var result = $"{World.Id}:{Index}:{Version}";
        var location = World.Entities.GetLocation(EntityId.Index);
        if (EntityId.Version != location.Version)
        {
            result += " (Destroyed)";
        }
        else if (location.Archetype == null!)
        {
            result += " (Pending)";
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Entity other)
    {
        return EntityId.CompareTo(other.EntityId);
    }
}
