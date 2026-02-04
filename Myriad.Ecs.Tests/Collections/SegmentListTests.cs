using System;
using Exanite.Myriad.Ecs.Collections;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests.Collections;

public class SegmentListTests
{
    [Fact]
    public void Create()
    {
        var list = new SegmentedList<int>(128);

        Assert.Equal(128, list.SegmentCapacity);
    }

    [Fact]
    public void IndexSingleSegment()
    {
        var list = new SegmentedList<int>(16);

        // Write index to each slot
        for (var i = 0; i < list.SegmentCapacity; i++)
        {
            list[i] = i;
        }

        // Read index from each slot
        for (var i = 0; i < list.SegmentCapacity; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void IndexSingleSegmentOutOfRange()
    {
        var list = new SegmentedList<int>(16);

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _ = list[-1];
        });

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _ = list[16];
        });
    }

    [Fact]
    public void Grow()
    {
        var list = new SegmentedList<int>(16);

        Assert.Equal(16, list.SegmentCapacity);
        Assert.Equal(16, list.TotalCapacity);
        Assert.Equal(0, list[15]);

        list.Grow();
        Assert.Equal(16, list.SegmentCapacity);
        Assert.Equal(32, list.TotalCapacity);
        Assert.Equal(0, list[31]);

        list.Grow();
        Assert.Equal(16, list.SegmentCapacity);
        Assert.Equal(48, list.TotalCapacity);
        Assert.Equal(0, list[47]);
    }
}
