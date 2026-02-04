using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// A list which stores data in "segments", this removes the need for copying data when the list grows.
/// </summary>
internal class SegmentedList<T>
{
    private readonly Lock growLock = new();

    private volatile T[][] segments = [];
    private readonly int shift;
    private readonly int mask;

    /// <summary>
    /// The capacity of a single segment within the list.
    /// </summary>
    public int SegmentCapacity { get; }

    /// <summary>
    /// The total capacity of the list.
    /// </summary>
    public int TotalCapacity => segments.Length * SegmentCapacity;

    public SegmentedList(int segmentCapacity)
    {
        GuardUtility.IsTrue(segmentCapacity.IsPowerOfTwo(), "Segment capacity must be a power of 2");

        SegmentCapacity = segmentCapacity;
        shift = BitOperations.TrailingZeroCount(segmentCapacity);
        mask = segmentCapacity - 1;

        Grow();
    }

    /// <summary>
    /// Get a reference to the item with the specified index.
    /// </summary>
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var localSegments = segments;

            var segmentIndex = index >> shift;
            var segment = localSegments[segmentIndex];
            var rowIndex = index & mask;

            return ref segment[rowIndex];
        }
    }

    /// <summary>
    /// Add another segment.
    /// </summary>
    public void Grow()
    {
        using var _ = growLock.EnterScope();
        
        T[][] newSegments = [..segments, new T[SegmentCapacity]];
        Interlocked.Exchange(ref segments, newSegments);
    }
}
