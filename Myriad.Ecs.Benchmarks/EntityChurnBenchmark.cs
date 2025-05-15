using BenchmarkDotNet.Attributes;
using Exanite.Myriad.Ecs.Benchmarks.Components;
using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Benchmarks;

[MemoryDiagnoser]
public class EntityChurnBenchmark
{
    private EcsWorld world = null!;
    private EcsCommandBuffer buffer = null!;
    private readonly List<BufferedEntity> buffered = [];

    [GlobalSetup]
    public void Setup()
    {
        world = new EcsWorld();
        buffer = new EcsCommandBuffer(world);
    }

    [Benchmark]
    public void Churn()
    {
        // keep track of every single entity currently alive
        var alive = new List<Entity>();

        // Do lots of rounds of creation and destruction
        for (var i = 0; i < 500000; i++)
        {
            // Create some entities
            for (var j = 0; j < 100; j++)
            {
                buffered.Add(buffer.Create().Set(new ComponentInt32(j)));
            }

            // Destroy all previously created entities
            buffer.Destroy(alive);
            alive.Clear();

            // Execute
            using var resolver = buffer.Execute();

            // Resolve results
            foreach (var b in buffered)
            {
                alive.Add(b.Resolve());
            }

            buffered.Clear();
        }
    }
}
