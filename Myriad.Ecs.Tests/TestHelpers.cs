using System;
using System.Collections.Generic;

namespace Exanite.Myriad.Ecs.Tests;

public static class TestHelpers
{
    public static List<Entity> AddRandomEntities(EcsWorld world, uint uniqueComponents = 7, int count = 1_000_000)
    {
        uniqueComponents = Math.Clamp(uniqueComponents, 0, 7);

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var random = new Random(123);

        var entities = new List<Entity>();
        for (var i = 0; i < count; i++)
        {
            var entity = commandBuffer.Create();
            entities.Add(entity);

            for (var j = 0; j < 5; j++)
            {
                switch (random.Next(0, checked((int)uniqueComponents)))
                {
                    case 0: entity.Set(new EcsByte(0)); break;
                    case 1: entity.Set(new EcsInt16(0)); break;
                    case 2: entity.Set(new EcsFloat(0)); break;
                    case 3: entity.Set(new EcsInt32(0)); break;
                    case 4: entity.Set(new EcsInt64(0)); break;
                    case 5: entity.Set(new Ecs0()); break;
                    case 6: entity.Set(new Ecs1()); break;
                }
            }
        }

        commandBuffer.Execute();

        return entities;
    }
}
