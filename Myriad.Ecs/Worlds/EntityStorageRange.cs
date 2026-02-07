using System;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Specifies a range of stored entities and contains utility methods for manipulating that range.
/// </summary>
internal readonly ref struct EntityStorageRange
{
    public readonly ref readonly EntityStorage Storage;
    public readonly int StartIndex;
    public readonly int Length;

    public EntityStorageRange(ref readonly EntityStorage storage, int startIndex, int length)
    {
        StartIndex = startIndex;
        Length = length;
        Storage = ref storage;
    }

    /// <summary>
    /// Copies all entity and component data from this range to the destination range.
    /// </summary>
    /// <remarks>
    /// Assumptions:
    /// <br/>
    /// The length of the source range determines the copy length.
    /// <br/>
    /// The two ranges are from the same archetype.
    /// </remarks>
    public void CopyAllTo(EntityStorageRange dstRange)
    {
        Array.Copy(Storage.EntityColumn, StartIndex, dstRange.Storage.EntityColumn, dstRange.StartIndex, Length);
        for (var i = 0; i < Storage.ComponentColumns.Length; i++)
        {
            var srcComponentColumn = Storage.ComponentColumns[i];
            var dstComponentColumn = dstRange.Storage.ComponentColumns[i];
            Array.Copy(srcComponentColumn, StartIndex, dstComponentColumn, dstRange.StartIndex, Length);
        }
    }

    /// <summary>
    /// Copies all component data from this range to the destination range.
    /// </summary>
    /// <remarks>
    /// Assumptions:
    /// <br/>
    /// The length of the source range determines the copy length.
    /// <br/>
    /// The two ranges are from the same archetype.
    /// </remarks>
    public void CopyComponentsTo(EntityStorageRange dstRange)
    {
        for (var i = 0; i < Storage.ComponentColumns.Length; i++)
        {
            var srcComponentColumn = Storage.ComponentColumns[i];
            var dstComponentColumn = dstRange.Storage.ComponentColumns[i];
            Array.Copy(srcComponentColumn, StartIndex, dstComponentColumn, dstRange.StartIndex, Length);
        }
    }

    /// <summary>
    /// Clears all entity and component data stored in this range.
    /// </summary>
    public void Clear()
    {
        Array.Clear(Storage.EntityColumn, StartIndex, Length);
        foreach (var componentColumn in Storage.ComponentColumns)
        {
            Array.Clear(componentColumn, StartIndex, Length);
        }
    }
}
