using BenchmarkDotNet.Attributes;
using Exanite.Myriad.Ecs.Benchmarks.Components;
using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class EntityCreateBenchmark
{
    private const int Count = 1_000_000;

    private EcsWorld world = null!;

    [GlobalSetup]
    public void Setup()
    {
        world = new EcsWorld();
    }

    private static void AddEntity(EcsCommandBuffer buffer, Random random)
    {
        var entity = buffer.Create();

        for (var i = 0; i < 5; i++)
        {
            switch (random.Next(0, 5))
            {
                case 0: entity.Set(new ComponentByte((byte)i)); break;
                case 1: entity.Set(new ComponentInt16((short)i)); break;
                case 2: entity.Set(new ComponentFloat(i)); break;
                case 3: entity.Set(new ComponentInt32(i)); break;
                case 4: entity.Set(new ComponentInt64(i)); break;
            }
        }
    }

    [Benchmark]
    public void CreateBuffered()
    {
        var rng = new Random(1);
        var buffer = new EcsCommandBuffer(world);

        for (var i = 0; i < Count; i++)
        {
            AddEntity(buffer, rng);
        }

        using var resolver = buffer.Execute();
    }

    [Benchmark]
    public void CreateUnbuffered()
    {
        var rng = new Random(1);
        var buffer = new EcsCommandBuffer(world);

        for (var i = 0; i < Count; i++)
        {
            AddEntity(buffer, rng);
            using var resolver = buffer.Execute();
        }
    }
}
