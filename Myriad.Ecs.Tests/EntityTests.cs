using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EntityTests
{
    [Fact]
    public void DefaultEntityIsNotAlive()
    {
        Assert.False(default(Entity).IsAlive);
        Assert.False(default(Entity).IsAlive);
    }

    [Fact]
    public void CompareDefaultEntity()
    {
        Assert.Equal(0, default(Entity).CompareTo(default));
    }

    [Fact]
    public void CompareEntityWithSelf()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create().Entity;
        commandBuffer.Execute();

        Assert.Equal(0, entity.CompareTo(entity));
    }

    [Fact]
    public void CompareEntityWithAnother()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity1 = commandBuffer.Create().Entity;
        var entity2 = commandBuffer.Create().Entity;

        commandBuffer.Execute();

        var c1 = entity1.CompareTo(entity2);
        var c2 = entity2.CompareTo(entity1);

        Assert.NotEqual(c1, c2);
        Assert.NotEqual(0, c1);
        Assert.NotEqual(0, c2);

        Assert.NotEqual(entity1.ToString(), entity2.ToString());
    }

    [Fact]
    public void GetComponent()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create()
            .Set(new EcsInt16(7))
            .Entity;

        commandBuffer.Execute();

        ref var c = ref entity.Get<EcsInt16>();
        Assert.Equal(7, c.Value);
    }

    [Fact]
    public void GetComponents()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create()
            .Set(new EcsInt16(7))
            .Entity;

        commandBuffer.Execute();

        Assert.Equal(1, entity.ComponentIds.Count);
        Assert.True(entity.ComponentIds.Contains(ComponentId.Get<EcsInt16>()));
    }

    [Fact]
    public void GetComponentDead()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create()
            .Set(new EcsInt16(7))
            .Entity;

        commandBuffer.Execute();

        commandBuffer.Destroy(entity);
        commandBuffer.Execute();

        Assert.Throws<GuardException>(() =>
        {
            _ = entity.ComponentIds.Count;
        });
    }

    [Fact]
    public void GetBoxedComponents()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create()
            .Set(new EcsInt16(7))
            .Entity;

        commandBuffer.Execute();

        Assert.Equal(1, entity.BoxedComponents.Length);
        Assert.Equal(new EcsInt16(7), (EcsInt16)entity.BoxedComponents[0]);
    }

    [Fact]
    public void GetBoxedComponent()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create()
            .Set(new EcsInt16(7))
            .Entity;

        commandBuffer.Execute();

        var component = (EcsInt16)entity.GetBoxed(ComponentId.Get<EcsInt16>())!;
        Assert.Equal(7, component.Value);

        Assert.Null(entity.GetBoxed(ComponentId.Get<EcsInt32>()));

        commandBuffer.Destroy(entity);
        commandBuffer.Execute();

        Assert.Null(entity.GetBoxed(ComponentId.Get<EcsInt16>()));
        Assert.Null(entity.GetBoxed(ComponentId.Get<EcsInt32>()));
    }
}
