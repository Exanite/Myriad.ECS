using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class ArchetypeTests
{
    [Fact]
    public void EnumerateManyEntities()
    {
        var world = new EcsWorld();

        var commandBuffer = world.AcquireCommandBuffer();
        for (var i = 0; i < 10000; i++)
        {
            var entity = commandBuffer.Create()
                .Set(new EcsInt32(i))
                .Set(new EcsInt64(i));

            if (i % 7 == 0)
            {
                entity.Set(new EcsFloat(i));
            }
        }
        commandBuffer.Execute();


        var query = new QueryFilter().Include<EcsInt32>().Include<EcsInt64>().Build(world);
        Assert.Equal(10000, query.Count());

        foreach (var archetype in query.Archetypes)
        {
            var a = archetype.GetSpan<EcsInt32>();
            var b = archetype.GetSpan<EcsInt64>();
            for (var i = 0; i < archetype.Entities.Length; i++)
            {
                Assert.Equal(a[i].Value, (int)b[i].Value);
            }
        }
    }
}
