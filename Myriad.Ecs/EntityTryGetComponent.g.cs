using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs;
using Exanite.Myriad.Ecs.Collections;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    public bool TryGetComponent<T0>(out ValueRef<T0> ref0) where T0 : IComponent
    {
        ref0 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();

        return true;
    }

    public bool TryGetComponent<T0, T1>(out ValueRef<T0> ref0, out ValueRef<T1> ref1) where T0 : IComponent where T1 : IComponent
    {
        ref0 = default;
        ref1 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2) where T0 : IComponent where T1 : IComponent where T2 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent
    {
        ref0 = default;
        ref1 = default;
        ref2 = default;
        ref3 = default;
        ref4 = default;
        ref5 = default;
        ref6 = default;
        ref7 = default;

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10, out ValueRef<T11> ref11) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        if (!HasComponent<T11>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();
        ref11 = GetComponentRef<T11>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10, out ValueRef<T11> ref11, out ValueRef<T12> ref12) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        if (!HasComponent<T11>())
        {
            return false;
        }

        if (!HasComponent<T12>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();
        ref11 = GetComponentRef<T11>();
        ref12 = GetComponentRef<T12>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10, out ValueRef<T11> ref11, out ValueRef<T12> ref12, out ValueRef<T13> ref13) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        if (!HasComponent<T11>())
        {
            return false;
        }

        if (!HasComponent<T12>())
        {
            return false;
        }

        if (!HasComponent<T13>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();
        ref11 = GetComponentRef<T11>();
        ref12 = GetComponentRef<T12>();
        ref13 = GetComponentRef<T13>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10, out ValueRef<T11> ref11, out ValueRef<T12> ref12, out ValueRef<T13> ref13, out ValueRef<T14> ref14) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        if (!HasComponent<T11>())
        {
            return false;
        }

        if (!HasComponent<T12>())
        {
            return false;
        }

        if (!HasComponent<T13>())
        {
            return false;
        }

        if (!HasComponent<T14>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();
        ref11 = GetComponentRef<T11>();
        ref12 = GetComponentRef<T12>();
        ref13 = GetComponentRef<T13>();
        ref14 = GetComponentRef<T14>();

        return true;
    }

    public bool TryGetComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(out ValueRef<T0> ref0, out ValueRef<T1> ref1, out ValueRef<T2> ref2, out ValueRef<T3> ref3, out ValueRef<T4> ref4, out ValueRef<T5> ref5, out ValueRef<T6> ref6, out ValueRef<T7> ref7, out ValueRef<T8> ref8, out ValueRef<T9> ref9, out ValueRef<T10> ref10, out ValueRef<T11> ref11, out ValueRef<T12> ref12, out ValueRef<T13> ref13, out ValueRef<T14> ref14, out ValueRef<T15> ref15) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent where T15 : IComponent
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

        if (!HasComponent<T0>())
        {
            return false;
        }

        if (!HasComponent<T1>())
        {
            return false;
        }

        if (!HasComponent<T2>())
        {
            return false;
        }

        if (!HasComponent<T3>())
        {
            return false;
        }

        if (!HasComponent<T4>())
        {
            return false;
        }

        if (!HasComponent<T5>())
        {
            return false;
        }

        if (!HasComponent<T6>())
        {
            return false;
        }

        if (!HasComponent<T7>())
        {
            return false;
        }

        if (!HasComponent<T8>())
        {
            return false;
        }

        if (!HasComponent<T9>())
        {
            return false;
        }

        if (!HasComponent<T10>())
        {
            return false;
        }

        if (!HasComponent<T11>())
        {
            return false;
        }

        if (!HasComponent<T12>())
        {
            return false;
        }

        if (!HasComponent<T13>())
        {
            return false;
        }

        if (!HasComponent<T14>())
        {
            return false;
        }

        if (!HasComponent<T15>())
        {
            return false;
        }

        ref0 = GetComponentRef<T0>();
        ref1 = GetComponentRef<T1>();
        ref2 = GetComponentRef<T2>();
        ref3 = GetComponentRef<T3>();
        ref4 = GetComponentRef<T4>();
        ref5 = GetComponentRef<T5>();
        ref6 = GetComponentRef<T6>();
        ref7 = GetComponentRef<T7>();
        ref8 = GetComponentRef<T8>();
        ref9 = GetComponentRef<T9>();
        ref10 = GetComponentRef<T10>();
        ref11 = GetComponentRef<T11>();
        ref12 = GetComponentRef<T12>();
        ref13 = GetComponentRef<T13>();
        ref14 = GetComponentRef<T14>();
        ref15 = GetComponentRef<T15>();

        return true;
    }

}
