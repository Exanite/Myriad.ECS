using BenchmarkDotNet.Attributes;
using Exanite.Myriad.Ecs.Benchmarks.Components;
using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Benchmarks;

[MemoryDiagnoser]
//[ShortRunJob]
public class EntityModifyBenchmark
{
    private const int Count = 1_000_000;

    private EcsWorld world = null!;
    private readonly List<Entity> entities = [];
    private EcsCommandBuffer ready = null!;

    [GlobalSetup]
    public void Setup()
    {
        world = new EcsWorld();

        // Create initial entities
        var rng = new Random(1);
        var buffer = new EcsCommandBuffer(world);
        for (var i = 0; i < Count; i++)
        {
            CreateEntity(buffer, rng);
        }

        using var resolver = buffer.Execute();
        for (var i = 0; i < resolver.Count; i++)
        {
            entities.Add(resolver[i]);
        }
    }

    private static void CreateEntity(EcsCommandBuffer buffer, Random random)
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

    private static void ModifyEntity(EcsCommandBuffer buffer, Random random, Entity entity)
    {
        for (var i = 0; i < 5; i++)
        {
            switch (random.Next(0, 6))
            {
                case 0: buffer.Set(entity, new ComponentByte((byte)i)); break;
                case 1: buffer.Set(entity, new ComponentInt16((short)i)); break;
                case 2: buffer.Set(entity, new ComponentFloat(i)); break;
                case 3: buffer.Set(entity, new ComponentInt32(i)); break;
                case 4: buffer.Set(entity, new ComponentInt64(i)); break;
                case 5: break;
            }
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Setup modifications in a buffer
        var rng = new Random();
        var ready = new EcsCommandBuffer(world);
        foreach (var entity in entities)
        {
            ModifyEntity(ready, rng, entity);
        }

        this.ready = ready;
    }

    [Benchmark]
    public void Playback()
    {
        ready.Execute().Dispose();
    }
}
