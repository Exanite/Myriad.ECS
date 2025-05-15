using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class EntityTests
{
    [TestMethod]
    public void DefaultEntityIsNotAlive()
    {
        Assert.IsFalse(default(Entity).IsAlive);
        Assert.IsFalse(default(Entity).IsAlive);
    }

    [TestMethod]
    public void CompareDefaultEntity()
    {
        Assert.AreEqual(0, default(Entity).CompareTo(default));
    }

    [TestMethod]
    public void CompareEntityWithSelf()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var eb = b.Create();
        using var resolver = b.Execute();
        var entity = eb.Resolve();

        Assert.AreEqual(0, entity.CompareTo(entity));
    }

    [TestMethod]
    public void CompareEntityWithAnother()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var eb1 = b.Create();
        var eb2 = b.Create();
        using var resolver = b.Execute();
        var entity1 = eb1.Resolve();
        var entity2 = eb2.Resolve();

        var c1 = entity1.CompareTo(entity2);
        var c2 = entity2.CompareTo(entity1);

        Assert.AreNotEqual(c1, c2);
        Assert.AreNotEqual(0, c1);
        Assert.AreNotEqual(0, c2);

        Assert.AreNotEqual(entity1.ToString(), entity2.ToString());
    }

    [TestMethod]
    public void GetComponent()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var e = b.Create()
                 .Set(new ComponentInt16(7));
        using var resolver = b.Execute();
        var entity = e.Resolve();

        ref var c = ref entity.GetComponent<ComponentInt16>();
        Assert.AreEqual(7, c.Value);
    }

    [TestMethod]
    public void GetComponents()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var e = b.Create().Set(new ComponentInt16(7));
        using var resolver = b.Execute();
        var entity = e.Resolve();

        Assert.AreEqual(1, entity.ComponentIds.Count);
        Assert.IsTrue(entity.ComponentIds.Contains(ComponentId.Get<ComponentInt16>()));
    }

    [TestMethod]
    public void GetComponentDead()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var e = b.Create().Set(new ComponentInt16(7));
        var resolver = b.Execute();
        var entity = e.Resolve();
        resolver.Dispose();

        b.Destroy(entity);
        b.Execute().Dispose();

        Assert.ThrowsException<ArgumentException>(() =>
        {
            var c = entity.ComponentIds.Count;
        });
    }

    [TestMethod]
    public void GetBoxedComponents()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var e = b.Create().Set(new ComponentInt16(7));
        using var resolver = b.Execute();
        var entity = e.Resolve();

        Assert.AreEqual(1, entity.BoxedComponents.Length);
        Assert.AreEqual(new ComponentInt16(7), (ComponentInt16)entity.BoxedComponents[0]);
    }

    [TestMethod]
    public void GetBoxedComponent()
    {
        var w = new EcsWorld();
        var b = new EcsCommandBuffer(w);

        var e = b.Create()
                 .Set(new ComponentInt16(7));
        using var resolver = b.Execute();
        var entity = e.Resolve();

        var c = (ComponentInt16)entity.GetBoxedComponent(ComponentId.Get<ComponentInt16>())!;
        Assert.AreEqual(7, c.Value);

        Assert.IsNull(entity.GetBoxedComponent(ComponentId.Get<ComponentInt32>()));

        b.Destroy(entity);
        b.Execute().Dispose();

        Assert.IsNull(entity.GetBoxedComponent(ComponentId.Get<ComponentInt16>()));
        Assert.IsNull(entity.GetBoxedComponent(ComponentId.Get<ComponentInt32>()));
    }
}
