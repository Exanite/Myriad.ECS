using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Myriad.Ecs.Components;
using Myriad.Ecs.xxHash;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// Probabilistic set of component IDs. Can be used to check if two sets intersect.<br/>
///
/// False positives are possible (i.e. If Intersects returns true, then there <b>might</b> be an overlap).<br/>
/// False negatives are <b>not</b> possible (i.e. If Intersects return false, then there <b>definitely</b> is no overlap).<br/>
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 0)]
internal struct ComponentBloomFilter
{
    private ulong a;
    private ulong b;
    private ulong c;
    private ulong d;
    private ulong e;
    private ulong f;

    public void Add(ComponentId id)
    {
        Span<int> value = stackalloc int[] { id.Value };
        var bytes = MemoryMarshal.Cast<int, byte>(value);

        // Set one random bit in each of the bitsets
        SetRandomBit(bytes, 136920569, ref a);
        SetRandomBit(bytes, 803654167, ref b);
        SetRandomBit(bytes, 786675075, ref c);
        SetRandomBit(bytes, 562713536, ref d);
        SetRandomBit(bytes, 703121798, ref e);
        SetRandomBit(bytes, 133703782, ref f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SetRandomBit(Span<byte> bytes, ulong seed, ref ulong output)
        {
            var hash = xxHash64.ComputeHash(bytes, seed) & 0b0011_1111;
            output |= 1UL << (int)hash;
        }
    }

    /// <summary>
    /// Check if this bloom filter <b>might</b> intersect with another.
    /// <br/>
    /// False positives are possible (i.e. If Intersects returns true, then there <b>might</b> be an overlap).
    /// <br/>
    /// False negatives are <b>not</b> possible (i.e. If Intersects return false, then there <b>definitely</b> is no overlap).
    /// </summary>
    public readonly bool MaybeIntersects(ref readonly ComponentBloomFilter other)
    {
        // The same items have been added to all 6 sets, with different hashes.
        // Therefore if _any_ of the sets do not intersect, then the overall
        // set does not intersect.

        // Bitwise & each matching element in the two sets together
        var abcd = System.Runtime.Intrinsics.Vector256.Create(a, b, c, d)
                 & System.Runtime.Intrinsics.Vector256.Create(other.a, other.b, other.c, other.d);
        var ef = System.Runtime.Intrinsics.Vector128.Create(e, f)
               & System.Runtime.Intrinsics.Vector128.Create(other.e, other.f);

        // Check if any of the elements had any matching bits
        var abz = System.Runtime.Intrinsics.Vector256.EqualsAny(abcd, System.Runtime.Intrinsics.Vector256<ulong>.Zero);
        var efz = System.Runtime.Intrinsics.Vector128.EqualsAny(ef, System.Runtime.Intrinsics.Vector128<ulong>.Zero);

        var fail = abz | efz;

        return !fail;
    }

    public void Union(ref readonly ComponentBloomFilter other)
    {
        a |= other.a;
        b |= other.b;
        c |= other.c;
        d |= other.d;
        e |= other.e;
        f |= other.f;
    }
}
