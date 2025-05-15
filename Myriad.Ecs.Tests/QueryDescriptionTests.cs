using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class QueryDescriptionTests
{
    [TestMethod]
    public void IsIncluded()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<ComponentFloat>()
               .Build(w);

        Assert.IsTrue(q.IsIncluded<ComponentFloat>());
        Assert.IsTrue(q.IsIncluded(typeof(ComponentFloat)));
        Assert.IsFalse(q.IsIncluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentInt32)));
    }

    [TestMethod]
    public void IsExcluded()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Exclude<ComponentFloat>()
               .Build(w);

        Assert.IsTrue(q.IsExcluded<ComponentFloat>());
        Assert.IsTrue(q.IsExcluded(typeof(ComponentFloat)));
        Assert.IsFalse(q.IsExcluded<ComponentInt32>());
        Assert.IsFalse(q.IsExcluded(typeof(ComponentInt32)));
    }

    [TestMethod]
    public void IsExactlyOneOf()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .ExactlyOneOf<ComponentFloat>()
               .ExactlyOneOf<ComponentByte>()
               .Build(w);

        Assert.IsTrue(q.IsExactlyOneOf<ComponentFloat>());
        Assert.IsTrue(q.IsExactlyOneOf(typeof(ComponentFloat)));
        Assert.IsFalse(q.IsExactlyOneOf<ComponentInt32>());
        Assert.IsFalse(q.IsExactlyOneOf(typeof(ComponentInt32)));
    }

    [TestMethod]
    public void IsAtLeastOneOf()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .AtLeastOneOf<ComponentFloat>()
               .AtLeastOneOf<ComponentByte>()
               .Build(w);

        Assert.IsTrue(q.IsAtLeastOneOf<ComponentFloat>());
        Assert.IsTrue(q.IsAtLeastOneOf(typeof(ComponentFloat)));
        Assert.IsFalse(q.IsAtLeastOneOf<ComponentInt32>());
        Assert.IsFalse(q.IsAtLeastOneOf(typeof(ComponentInt32)));
    }

    [TestMethod]
    public void IncludeMatchNone()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var a = q.GetArchetypeMatches();

        Assert.IsNotNull(a);
        Assert.AreEqual(0, a.Count);
    }

    [TestMethod]
    public void IncludeMatchNoneNonGeneric()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
           .Include(typeof(ComponentFloat))
           .Build(w);

        var a = q.GetArchetypeMatches();

        Assert.IsNotNull(a);
        Assert.AreEqual(0, a.Count);
    }

    [TestMethod]
    public void IncludeMatchOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var a = q.GetArchetypeMatches();

        Assert.IsNotNull(a);
        Assert.AreEqual(1, a.Count);
        Assert.IsNull(a.Single().AtLeastOne);
        Assert.IsNull(a.Single().ExactlyOne);
    }

    [TestMethod]
    public void IncludeMatchCaching()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);

        // Query that matches just one of the archetypes in the world
        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        // Match once, check it matches one archetype
        var a = q.GetArchetypeMatches();
        Assert.IsNotNull(a);
        Assert.AreEqual(1, a.Count);
        Assert.IsNull(a.Single().AtLeastOne);
        Assert.IsNull(a.Single().ExactlyOne);

        // Add an archetype to the world that the query should match
        var c1 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentFloat>() });
        w.GetOrCreateArchetype(c1, ArchetypeHash.Create(c1));

        // Check it now matches 2 archetypes
        var b = q.GetArchetypeMatches();
        Assert.IsNotNull(b);
        Assert.AreEqual(2, b.Count);
        Assert.IsTrue(a.All(x => x.Archetype.Components.Contains(ComponentId.Get<ComponentFloat>())));

        // Add an archetype to the world that the query should NOT match
        var c2 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentByte>() });
        w.GetOrCreateArchetype(c2, ArchetypeHash.Create(c2));

        // Check it now matches 2 archetypes
        var c = q.GetArchetypeMatches();
        Assert.IsNotNull(c);
        Assert.AreEqual(2, c.Count);
        Assert.IsTrue(c.All(x => x.Archetype.Components.Contains(ComponentId.Get<ComponentFloat>())));
    }

    [TestMethod]
    public void IncludeMatchMultiple()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var a = q.GetArchetypeMatches();

        Assert.IsNotNull(a);
        Assert.AreEqual(2, a.Count);

        Assert.IsTrue(a.All(x => x.Archetype.Components.Contains(ComponentId.Get<ComponentFloat>())));
    }

    [TestMethod]
    public void IncludeExclude()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
            .Include<ComponentFloat>()
            .Exclude<ComponentInt32>()
            .Build(w);

        var a = q.GetArchetypeMatches();

        Assert.IsNotNull(a);
        Assert.AreEqual(1, a.Count);

        var single = a.Single();
        Assert.IsNull(single.AtLeastOne);
        Assert.IsNull(single.ExactlyOne);
        Assert.IsTrue(single.Archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        Assert.IsFalse(single.Archetype.Components.Contains(ComponentId.Get<ComponentInt32>()));
    }

    [TestMethod]
    public void ExactlyOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt16>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
                .ExactlyOneOf<ComponentFloat>()
                .ExactlyOneOf<ComponentInt32>()
                .Build(w);

        var matches = q.GetArchetypeMatches();

        Assert.IsNotNull(matches);
        Assert.AreEqual(2, matches.Count);

        foreach (var match in matches)
        {
            Assert.IsNotNull(match);
            Assert.IsTrue(match.ExactlyOne == ComponentId.Get<ComponentInt32>() || match.ExactlyOne == ComponentId.Get<ComponentFloat>());
            Assert.IsTrue(match.AtLeastOne == null);
            Assert.IsTrue(match.Archetype.Components.Count == 1);
        }
    }

    [TestMethod]
    public void AtLeastOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt16>(), ComponentId.Get<ComponentInt64>()]);

        var q = new QueryBuilder()
            .AtLeastOneOf<ComponentFloat>()
            .AtLeastOneOf<ComponentInt32>()
            .Build(w);

        var matches = q.GetArchetypeMatches();

        Assert.IsNotNull(matches);
        Assert.AreEqual(3, matches.Count);

        foreach (var match in matches)
        {
            Assert.IsNotNull(match);
            Assert.IsTrue(match.ExactlyOne == null);

            Assert.IsTrue(match.Archetype.Components.Contains(ComponentId.Get<ComponentInt32>())
                       || match.Archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        }
    }

    [TestMethod]
    public void First_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.ThrowsException<InvalidOperationException>(() => q.First());
    }

    [TestMethod]
    public void First_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.First());
    }

    [TestMethod]
    public void First_MatchMultiple()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb1 = c.Create().Set(new Component0());
        var eb2 = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e1 = eb1.Resolve();
        var e2 = eb2.Resolve();

        Assert.IsTrue(new[] { e1, e2 }.Contains(q.First()));
    }

    [TestMethod]
    public void FirstOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsNull(q.FirstOrDefault());
    }

    [TestMethod]
    public void FirstOrDefault_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.FirstOrDefault());
    }

    [TestMethod]
    public void FirstOrDefault_MatchMultiple()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb1 = c.Create().Set(new Component0());
        var eb2 = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e1 = eb1.Resolve();
        var e2 = eb2.Resolve();

        Assert.IsTrue(new[] { e1, e2 }.Contains(q.FirstOrDefault()!.Value));
    }

    [TestMethod]
    public void SingleOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsNull(q.SingleOrDefault());
    }

    [TestMethod]
    public void SingleOrDefault_ThrowsMultipleMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        c.Create().Set(new Component0());
        c.Create().Set(new Component0());
        c.Execute().Dispose();

        Assert.ThrowsException<InvalidOperationException>(() => q.SingleOrDefault());
    }

    [TestMethod]
    public void SingleOrDefault_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.SingleOrDefault()!.Value);
    }

    [TestMethod]
    public void Single_ThrowsMultipleMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        c.Create().Set(new Component0());
        c.Create().Set(new Component0());
        c.Execute().Dispose();

        Assert.ThrowsException<InvalidOperationException>(() => q.Single());
    }

    [TestMethod]
    public void Single_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.ThrowsException<InvalidOperationException>(() => q.Single());
    }

    [TestMethod]
    public void Single_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.Single());
    }

    [TestMethod]
    public void Any_True()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.IsTrue(q.Any());
    }

    [TestMethod]
    public void Any_False()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsFalse(q.Any());
    }

    [TestMethod]
    public void Contains_True()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.IsTrue(q.Contains(e));
    }

    [TestMethod]
    public void Contains_False()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create();
        using var _ = c.Execute();
        var e = eb.Resolve();

        Assert.IsFalse(q.Contains(e));
    }

    [TestMethod]
    public void Random_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var rng = new Random(123);

        Assert.IsNull(q.RandomOrDefault(rng));
    }

    [TestMethod]
    public void Random_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());
        using var _ = c.Execute();
        var e = eb.Resolve();

        var r = new Random(123);

        Assert.AreEqual(e, q.RandomOrDefault(r));
    }

    [TestMethod]
    public void Random_MatchRandom()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<ComponentInt32>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i));
        }

        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i)).Set(new Component0());
        }

        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i)).Set(new Component1());
        }

        using var resolver = c.Execute();
        var entities = new List<Entity>();
        for (var i = 0; i < resolver.Count; i++)
        {
            entities.Add(resolver[i]);
        }

        var r = new Random(123);

        for (var i = 0; i < 1000; i++)
        {
            Assert.IsTrue(entities.Contains(q.RandomOrDefault(r)!.Value));
        }
    }
}
