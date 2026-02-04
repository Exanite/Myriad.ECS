using System;
using System.Linq;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class ComponentRegistryTests
{
    [Fact]
    public void CannotAssignNonComponent()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ComponentId.Get(typeof(int));
        });
    }

    [Fact]
    public void AssignsDistinctIds()
    {
        var ids = new[]
        {
            ComponentId.Get<EcsInt32>(),
            ComponentId.Get<EcsInt64>(),
            ComponentId.Get(typeof(EcsInt16)),
            ComponentId.Get(typeof(EcsFloat)),
        };

        Assert.Equal(4, ids.Distinct().Count());

        Assert.Equal(typeof(EcsInt16), ComponentId.Get<EcsInt16>().Type);
    }

    [Fact]
    public void DoesNotReassign()
    {
        var id = ComponentId.Get<EcsInt32>();
        var id2 = ComponentId.Get<EcsInt32>();

        Assert.Equal(id, id2);
    }

    [Fact]
    public void ThrowsForUnknownId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = default(ComponentId).Type;
        });
    }
}
