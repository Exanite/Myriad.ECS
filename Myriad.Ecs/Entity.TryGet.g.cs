#nullable enable

using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0>(out Ref<T0> ref0) where T0 : IComponent
    {
        ref0 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1>(out Ref<T0> ref0, out Ref<T1> ref1) where T0 : IComponent where T1 : IComponent
    {
        ref0 = default;
        ref1 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2) where T0 : IComponent where T1 : IComponent where T2 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10, out Ref<T11> ref11) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;
        ref11 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = location.Archetype.GetRef<T11>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10, out Ref<T11> ref11, out Ref<T12> ref12) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;
        ref11 = default;
        ref12 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = location.Archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = location.Archetype.GetRef<T12>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10, out Ref<T11> ref11, out Ref<T12> ref12, out Ref<T13> ref13) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;
        ref11 = default;
        ref12 = default;
        ref13 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = location.Archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = location.Archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = location.Archetype.GetRef<T13>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10, out Ref<T11> ref11, out Ref<T12> ref12, out Ref<T13> ref13, out Ref<T14> ref14) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;
        ref11 = default;
        ref12 = default;
        ref13 = default;
        ref14 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        var componentId14 = ComponentId.Get<T14>();
        if (!components.Contains(componentId14))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = location.Archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = location.Archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = location.Archetype.GetRef<T13>(location.IndexInArchetype);
        ref14 = location.Archetype.GetRef<T14>(location.IndexInArchetype);

        return true;
    }

    /// <summary>
    /// Try to get a reference to a component of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(out Ref<T0> ref0, out Ref<T1> ref1, out Ref<T2> ref2, out Ref<T3> ref3, out Ref<T4> ref4, out Ref<T5> ref5, out Ref<T6> ref6, out Ref<T7> ref7, out Ref<T8> ref8, out Ref<T9> ref9, out Ref<T10> ref10, out Ref<T11> ref11, out Ref<T12> ref12, out Ref<T13> ref13, out Ref<T14> ref14, out Ref<T15> ref15) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent where T15 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;
        ref8 = default;
        ref9 = default;
        ref10 = default;
        ref11 = default;
        ref12 = default;
        ref13 = default;
        ref14 = default;
        ref15 = default;

        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            return false;
        }

        ref var location = ref locationRef.Value;
        var components = location.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        var componentId14 = ComponentId.Get<T14>();
        if (!components.Contains(componentId14))
        {
            return false;
        }

        var componentId15 = ComponentId.Get<T15>();
        if (!components.Contains(componentId15))
        {
            return false;
        }

        ref0 = location.Archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = location.Archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = location.Archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = location.Archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = location.Archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = location.Archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = location.Archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = location.Archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = location.Archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = location.Archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = location.Archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = location.Archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = location.Archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = location.Archetype.GetRef<T13>(location.IndexInArchetype);
        ref14 = location.Archetype.GetRef<T14>(location.IndexInArchetype);
        ref15 = location.Archetype.GetRef<T15>(location.IndexInArchetype);

        return true;
    }

}
