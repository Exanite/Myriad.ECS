using System.Numerics;
using BenchmarkDotNet.Attributes;
using Exanite.Myriad.Ecs.Benchmarks.Components;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Queries;

namespace Exanite.Myriad.Ecs.Benchmarks;

//[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
//[ThreadingDiagnoser]
//[MemoryDiagnoser]
//[ShortRunJob]
public class QueryBenchmark
{
    [Params(100_000, 1_000_000)]
    public int EntityCount = 1_000_000;

    private EcsWorld world = null!;
    private QueryDescription query = null!;

    [GlobalSetup]
    public void Setup()
    {
        world = new EcsWorld();

        var buffer = new EcsCommandBuffer(world);
        var rng = new Random(2);
        for (var i = 0; i < EntityCount; i++)
        {
            AddEntity(buffer, rng);
        }

        using var resolver = buffer.Execute();

        query = new QueryBuilder()
            .Include<Position>()
            .Include<Velocity>()
            .Build(world);
        query.GetArchetypes();

        Console.WriteLine("Setup Complete");
    }

    private static void AddEntity(EcsCommandBuffer buffer, Random random)
    {
        var entity = buffer.Create();

        entity.Set(new Position(new Vector2(random.NextSingle(), random.NextSingle())));
        entity.Set(new Velocity(new Vector2(random.NextSingle(), random.NextSingle())));

        for (var i = 0; i < 10; i++)
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

    // [Benchmark]
    // public void Query()
    // {
    //     var q = new QueryAction();
    //     world.Execute<QueryAction, Position, Velocity>(ref q, query);
    // }

    // [Benchmark]
    // public void ChunkQuery()
    // {
    //     var q = new ChunkQueryAction();
    //     world.ExecuteChunk<ChunkQueryAction, Position, Velocity>(ref q, query);
    // }

    //[Benchmark]
    //public void SimdChunkQuery()
    //{
    //    _world.ExecuteVectorChunk<SimdChunkQueryAction2, Position, float, Velocity, float>(new SimdChunkQueryAction2(), _query);
    //}

    //[Benchmark]
    //public void ParallelQuery()
    //{
    //    _world.ExecuteParallel<QueryAction, Position, Velocity>(new QueryAction(), _query);
    //}

    //[Benchmark]
    //public void ParallelChunkQuery()
    //{
    //    _world.ExecuteChunkParallel<ChunkQueryAction, Position, Velocity>(new ChunkQueryAction(), _query);
    //}

    //[Benchmark]
    //public void QueryEnumerable()
    //{
    //    foreach (var item in _world.Query<Position, Velocity>(_query))
    //        item.Item0.Value += item.Item1.Value;
    //}

    //[Benchmark]
    //public void DelegateQuery()
    //{
    //    _world.Query((ref Position pos, ref Velocity vel) =>
    //    {
    //        pos.Value += vel.Value;
    //    });
    //}

    // private struct QueryAction
    //     : IQuery<Position, Velocity>
    // {
    //     public readonly void Execute(Entity e, ref Position pos, ref Velocity vel)
    //     {
    //         pos.Value += vel.Value;
    //         //pos.Value += new Vector2(
    //         //    (float)Math.Sqrt(Math.Abs(Math.Tanh(pos.Value.X))),
    //         //    (float)Math.Tanh(pos.Value.Y)
    //         //);
    //     }
    // }

    // private struct ChunkQueryAction
    //     : IChunkQuery<Position, Velocity>
    // {
    //     public readonly void Execute(ChunkHandle chunk, ReadOnlySpan<Entity> e, Span<Position> pos, Span<Velocity> vel)
    //     {
    //         for (var i = 0; i < pos.Length; i++)
    //         {
    //             ref var posi = ref pos[i];
    //
    //             posi.Value += vel[i].Value;
    //
    //             //posi.Value += new Vector2(
    //             //    (float)Math.Sqrt(Math.Abs(Math.Tanh(posi.Value.X))),
    //             //    (float)Math.Tanh(posi.Value.Y)
    //             //);
    //         }
    //     }
    // }

    // private readonly struct SimdChunkQueryAction2
    //     : IVectorChunkQuery<float, float>
    // {
    //     public void Execute(Span<Vector<float>> posf, Span<Vector<float>> velf, int offset, int pad)
    //     {
    //         for (var i = 0; i < posf.Length; i++)
    //             posf[i] += velf[i];
    //
    //         throw new NotImplementedException("implement extra complicated stuff");
    //     }
    // }
}
