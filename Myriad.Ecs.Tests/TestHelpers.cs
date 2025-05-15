using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Tests;

public static class TestHelpers
{
    public static EcsCommandBuffer SetupRandomEntities(EcsWorld world, uint uniqueComponents = 7, int count = 1_000_000)
    {
        uniqueComponents = Math.Clamp(uniqueComponents, 0, 7);

        var b = new EcsCommandBuffer(world);
        var r = new Random(3452);
        for (var i = 0; i < count; i++)
        {
            var eb = b.Create();

            for (var j = 0; j < 5; j++)
            {
                switch (r.Next(0, checked((int)uniqueComponents)))
                {
                    case 0: eb.Set(new ComponentByte(0)); break;
                    case 1: eb.Set(new ComponentInt16(0)); break;
                    case 2: eb.Set(new ComponentFloat(0)); break;
                    case 3: eb.Set(new ComponentInt32(0)); break;
                    case 4: eb.Set(new ComponentInt64(0)); break;
                    case 5: eb.Set(new Component0()); break;
                    case 6: eb.Set(new Component1()); break;
                }
            }
        }

        return b;
    }
}
