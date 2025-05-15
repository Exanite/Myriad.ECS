using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class ArchetypeTests
{
    [TestMethod]
    public void EnumerateManyEntities()
    {
        var w = new EcsWorld();

        var cb = new EcsCommandBuffer(w);
        for (var i = 0; i < 10000; i++)
        {
            var e = cb.Create();
            e.Set(new ComponentInt32(i)).Set(new ComponentInt64(i));
            if (i % 7 == 0)
            {
                e.Set(new ComponentFloat(i));
            }
        }
        cb.Execute().Dispose();

        foreach (var archetype in w.Archetypes)
        {
            var entityCount = 0;
            foreach (var chunk in archetype.Chunks)
            {
                entityCount += chunk.EntityCount;
            }

            Assert.AreEqual(archetype.EntityCount, entityCount);
        }

        var query = new QueryBuilder().Include<ComponentInt32>().Include<ComponentInt64>().Build(w);
        Assert.AreEqual(10000, query.Count());

        foreach (var archetypeMatch in query.GetArchetypeMatches())
        {
            foreach (var chunk in archetypeMatch.Archetype.Chunks)
            {
                var a = chunk.GetSpan<ComponentInt32>();
                var b = chunk.GetSpan<ComponentInt64>();

                for (var i = 0; i < chunk.EntityCount; i++)
                {
                    Assert.AreEqual(a[i].Value, (int)b[i].Value);
                }
            }
        }
    }
}
