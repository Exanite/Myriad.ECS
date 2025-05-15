using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests.Collections;

[TestClass]
public class ComponentBloomFilterTests
{
    [TestMethod]
    public void EmptyNotIntersect()
    {
        var a = new ComponentBloomFilter();
        var b = new ComponentBloomFilter();

        Assert.IsFalse(a.MaybeIntersects(ref b));
    }

    [TestMethod]
    public void DisjointNotIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component2>());
        a.Add(ComponentId.Get<Component3>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component8>());
        b.Add(ComponentId.Get<Component9>());
        b.Add(ComponentId.Get<Component10>());
        b.Add(ComponentId.Get<Component11>());

        var i = a.MaybeIntersects(ref b);
        Assert.IsFalse(i);

        // Note that this test _can_ fail, since the bloom filter is probabilistic and errors on the side of caution.
    }

    [TestMethod]
    public void IntersectingIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component4>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component2>());
        b.Add(ComponentId.Get<Component3>());
        b.Add(ComponentId.Get<Component4>());

        Assert.IsTrue(a.MaybeIntersects(ref b));
    }

    [TestMethod]
    public void UnionIntersects()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component2>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component3>());
        b.Add(ComponentId.Get<Component4>());
        b.Add(ComponentId.Get<Component5>());

        var c = new ComponentBloomFilter();
        c.Add(ComponentId.Get<Component0>());

        Assert.IsFalse(a.MaybeIntersects(ref b));
        Assert.IsFalse(b.MaybeIntersects(ref c));
        Assert.IsTrue(a.MaybeIntersects(ref c));

        var d = new ComponentBloomFilter();
        d.Union(ref b);
        d.Union(ref c);

        Assert.IsTrue(a.MaybeIntersects(ref d));
    }
}
