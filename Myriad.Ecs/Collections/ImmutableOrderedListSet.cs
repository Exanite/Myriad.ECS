using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// An immutable set of objects.
/// </summary>
public class ImmutableOrderedListSet<T> : IReadOnlyList<T>, IEquatable<ImmutableOrderedListSet<T>> where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// An empty set.
    /// </summary>
    public static readonly ImmutableOrderedListSet<T> Empty = new([]);

    private readonly OrderedListSet<T> items;
    private int? hashCode;

    public T this[int i] => items[i];

    public int Count => items.Count;

    private ImmutableOrderedListSet(OrderedListSet<T> dangerousItems)
    {
        items = dangerousItems;
    }

    public static ImmutableOrderedListSet<T> Create(List<T> items)
    {
        if (items.Count == 0)
        {
            return Empty;
        }

        return new ImmutableOrderedListSet<T>(new OrderedListSet<T>(items));
    }

    public static ImmutableOrderedListSet<T> Create(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
        {
            return Empty;
        }

        return new ImmutableOrderedListSet<T>(new OrderedListSet<T>(items));
    }

    public static ImmutableOrderedListSet<T> Create(HashSet<T> items)
    {
        if (items.Count == 0)
        {
            return Empty;
        }

        return new ImmutableOrderedListSet<T>(new OrderedListSet<T>(items));
    }

    public static ImmutableOrderedListSet<T> Create(OrderedListSet<T> items)
    {
        if (items.Count == 0)
        {
            return Empty;
        }

        return new ImmutableOrderedListSet<T>(new OrderedListSet<T>(items));
    }

    public static ImmutableOrderedListSet<T> Create<TValue>(Dictionary<T, TValue> items)
    {
        if (items.Count == 0)
        {
            return Empty;
        }

        var set = new OrderedListSet<T>();
        set.UnionWith(items);
        return new ImmutableOrderedListSet<T>(set);
    }

    /// <summary>
    /// Copy this set to the given list
    /// </summary>
    public void CopyTo(List<T> dest)
    {
        items.CopyTo(dest);
    }

    /// <summary>
    /// Get an enumerator over the items in this set
    /// </summary>
    public List<T>.Enumerator GetEnumerator()
    {
        // ReSharper disable once NotDisposedResourceIsReturned
        return items.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Check if this set contains the given item
    /// </summary>
    public bool Contains(T item)
    {
        return items.Contains(item);
    }

    // TODO: other set methods when needed

    //public bool IsProperSubsetOf(OrderedListSet<TItem> other)
    //{
    //    return _items.IsProperSubsetOf(other);
    //}

    //public bool IsProperSubsetOf(ImmutableOrderedListSet<TItem> other)
    //{
    //    return _items.IsProperSubsetOf(other._items);
    //}

    //public bool IsProperSupersetOf(OrderedListSet<TItem> other)
    //{
    //    return _items.IsProperSupersetOf(other);
    //}

    //public bool IsProperSupersetOf(ImmutableOrderedListSet<TItem> other)
    //{
    //    return _items.IsProperSupersetOf(other._items);
    //}

    //public bool IsSubsetOf(OrderedListSet<TItem> other)
    //{
    //    return _items.IsSubsetOf(other);
    //}

    //public bool IsSubsetOf(ImmutableOrderedListSet<TItem> other)
    //{
    //    return _items.IsSubsetOf(other._items);
    //}

    /// <summary>
    /// Check if this set is a superset of another set. i.e. contains all the items in the other set.
    /// </summary>
    public bool IsSupersetOf(OrderedListSet<T> other)
    {
        return items.IsSupersetOf(other);
    }

    /// <summary>
    /// Check if this set is a superset of another set. i.e. contains all the items in the other set.
    /// </summary>
    public bool IsSupersetOf(ImmutableOrderedListSet<T> other)
    {
        return IsSupersetOf(other.items);
    }

    /// <summary>
    /// Check if this set overlaps another set. i.e. contains at least one item which is in the other set.
    /// </summary>
    public bool Overlaps(OrderedListSet<T> other)
    {
        return items.Overlaps(other);
    }

    /// <summary>
    /// Check if this set overlaps another set. i.e. contains at least one item which is in the other set.
    /// </summary>
    public bool Overlaps(ImmutableOrderedListSet<T> other)
    {
        return Overlaps(other.items);
    }

    /// <summary>
    /// Check if this set contains exactly the same items as another set
    /// </summary>
    public bool SetEquals(OrderedListSet<T> other)
    {
        return items.SetEquals(other);
    }

    /// <summary>
    /// Check if this set contains exactly the same items as another set
    /// </summary>
    public bool SetEquals(ImmutableOrderedListSet<T> other)
    {
        return SetEquals(other.items);
    }

    /// <summary>
    /// Check if this set contains exactly the same items as another set
    /// </summary>
    public bool SetEquals<TValue>(Dictionary<T, TValue> other)
    {
        return items.SetEquals(other);
    }

    public bool Equals(ImmutableOrderedListSet<T>? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.Count != Count)
        {
            return false;
        }

        for (var i = 0; i < other.Count; i++)
        {
            if (!items[i].Equals(other[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is ImmutableOrderedListSet<T> other && Equals(other);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        if (hashCode.HasValue)
        {
            return hashCode.Value;
        }

        var result = 0;
        foreach (var item in items)
        {
            result = HashCode.Combine(result, item);
        }

        hashCode = result;
        return result;
    }
}
