using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class ArchetypeTests
{
    [Fact]
    public void EnumerateManyEntities()
    {
        var w = new EcsWorld();

        var cb = w.AcquireCommandBuffer();
        for (var i = 0; i < 10000; i++)
        {
            var e = cb.Create();
            e.Set(new EcsInt32(i)).Set(new EcsInt64(i));
            if (i % 7 == 0)
            {
                e.Set(new EcsFloat(i));
            }
        }
        cb.Execute();

        foreach (var archetype in w.Archetypes)
        {
            var entityCount = 0;
            foreach (var chunk in archetype.Chunks)
            {
                entityCount += chunk.Entities.Length;
            }

            Assert.Equal(archetype.EntityCount, entityCount);
        }

        var query = new QueryFilter().Include<EcsInt32>().Include<EcsInt64>().Build(w);
        Assert.Equal(10000, query.Count());

        foreach (var archetypeMatch in query.Archetypes)
        {
            foreach (var chunk in archetypeMatch.Chunks)
            {
                var a = chunk.GetSpan<EcsInt32>();
                var b = chunk.GetSpan<EcsInt64>();

                for (var i = 0; i < chunk.Entities.Length; i++)
                {
                    Assert.Equal(a[i].Value, (int)b[i].Value);
                }
            }
        }
    }
}
