using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests.Collections;

public class OrderedListSetTests
{
    [Fact]
    public void Create()
    {
        var set = new OrderedListSet<int>();

        Assert.Equal(0, set.Count);
    }

    [Fact]
    public void Create_NonEmpty()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        Assert.Equal(3, set.Count);
        Assert.True(set.Contains(1));
        Assert.True(set.Contains(2));
        Assert.True(set.Contains(3));
        Assert.False(set.Contains(4));
    }

    [Fact]
    public void UnionWith()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var ints = new OrderedListSet<int>
        {
            1,
            2,
            3,
            4,
        };

        var immutable = ImmutableOrderedListSet<int>.Create(ints);
        set.UnionWith(immutable);

        Assert.Equal(4, set.Count);
        Assert.True(set.Contains(1));
        Assert.True(set.Contains(2));
        Assert.True(set.Contains(3));
        Assert.True(set.Contains(4));
    }

    [Fact]
    public void AddUnique()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
            11,
            5,
        };

        Assert.Equal(5, set.Count);
    }

    [Fact]
    public void AddDuplicates()
    {
        var set = new OrderedListSet<int>
        {
            1,
            1,
            2,
            3,
            2,
            2,
            11,
        };

        Assert.Equal(4, set.Count);
    }

    [Fact]
    public void SetEquals_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            3,
            2,
            1,
        };

        Assert.True(a.SetEquals(b));
    }

    [Fact]
    public void SetEquals_False_SameCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            1,
            0,
        };

        Assert.False(a.ToImmutable().SetEquals(b.ToImmutable()));
    }

    [Fact]
    public void SetEquals_False_Superset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            1,
            2,
            3,
            4,
        };

        Assert.False(a.SetEquals(b));
    }

    [Fact]
    public void SetEquals_False_Subset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            1,
            2,
        };

        Assert.False(a.SetEquals(b));
    }

    [Fact]
    public void SetEquals_Enumerable_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            3,
            2,
            1,
        };

        Assert.True(a.SetEquals(b));
    }

    [Fact]
    public void SetEquals_Enumerable_False_SameCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            2,
            1,
            0,
        };

        Assert.False(a.SetEquals(b));
    }

    [Fact]
    public void SetEquals_Enumerable_False_DifferentCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            3,
            2,
            1,
            0,
        };

        Assert.False(a.SetEquals(b));
    }

    [Fact]
    public void IsSuperset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            3,
        };

        Assert.True(a.ToImmutable().IsSupersetOf(b.ToImmutable()));
        Assert.False(b.ToImmutable().IsSupersetOf(a.ToImmutable()));
    }

    [Fact]
    public void Overlaps_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            3,
        };

        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_False()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            4,
            5,
        };

        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_False_Empty()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>();

        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }
}
