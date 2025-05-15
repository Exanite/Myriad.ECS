using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class EcsCommandBufferTests
{
    [TestMethod]
    public void CreateCommandBuffer()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);
        Assert.IsNotNull(buffer);
    }

    [TestMethod]
    public void DisposeResolverTwiceThrows()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var r = buffer.Execute();
        r.Dispose();

        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            r.Dispose();
        });
    }

    [TestMethod]
    public void CreateEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var eb = buffer.Create();
        Assert.AreEqual(buffer, eb.CommandBuffer);

        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        Assert.IsTrue(entity.IsAlive);
        Assert.AreEqual(1, world.Archetypes.Count);
        Assert.AreEqual(0, world.Archetypes.Single().Components.Count);
    }

    [TestMethod]
    public void CreateManyEntities()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create lots of entities
        var buffered = new List<BufferedEntity>();
        for (var i = 0; i < 50000; i++)
        {
            buffered.Add(buffer.Create().Set(new ComponentInt32(i)));
        }

        // Execute buffer
        using var resolver = buffer.Execute();

        // Resolve results
        var entities = new List<Entity>();
        foreach (var b in buffered)
        {
            entities.Add(b.Resolve());
        }

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            Assert.IsTrue(entity.IsAlive);
            Assert.AreEqual(1, world.Archetypes.Count);
            Assert.AreEqual(1, world.Archetypes.Single().Components.Count);
            Assert.AreEqual(i, entity.GetComponent<ComponentInt32>().Value);
        }
    }

    [TestMethod]
    public void ChurnCreateDestroy()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var rng = new Random(46576);

        // keep track ofevery single entity ever created
        var alive = new List<Entity>();
        var dead = new List<Entity>();

        // Do lots of rounds of creation and destruction
        for (var i = 0; i < 20; i++)
        {
            // Create lots of entities
            var buffered = new List<BufferedEntity>();
            for (var j = 0; j < 10000; j++)
            {
                var b = buffer.Create().Set(new ComponentInt32(j));
                buffered.Add(b);

                for (int k = 0; k < 3; k++)
                {
                    switch (rng.Next(0, 6))
                    {
                        case 0: b.Set(new ComponentByte((byte)i)); break;
                        case 1: b.Set(new ComponentInt16((short)i)); break;
                        case 2: b.Set(new ComponentFloat(i)); break;
                        case 3: b.Set(new ComponentInt32(i)); break;
                        case 4: b.Set(new ComponentInt64(i)); break;
                    }
                }
            }

            // Destroy some random entities
            for (var j = 0; j < 1000; j++)
            {
                if (alive.Count == 0)
                {
                    break;
                }

                var index = rng.Next(0, alive.Count);
                var ent = alive[index];
                Assert.IsTrue(ent.IsAlive);
                buffer.Destroy(ent);
                alive.RemoveAt(index);
                dead.Add(ent);
            }

            // Execute
            using (buffer.Execute())
            {
                // Resolve results
                foreach (var b in buffered)
                {
                    alive.Add(b.Resolve());
                }
            }

            // Check all the entities
            for (var j = 0; j < alive.Count; j++)
            {
                var entity = alive[j];
                Assert.IsTrue(entity.IsAlive);
            }
            for (var j = 0; j < dead.Count; j++)
            {
                var entity = dead[j];
                Assert.IsTrue(!entity.IsAlive);
            }

            // Check archetypes
            Assert.AreEqual(alive.Count, world.Archetypes.Select(a => a.EntityCount).Sum());
        }
    }

    [TestMethod]
    public void ChurnStructural()
    {
        var world = new EcsWorld();
        var entities = new List<Entity>();
        using (var setupResolver = TestHelpers.SetupRandomEntities(world, 10_000).Execute())
        {
            for (var i = 0; i < setupResolver.Count; i++)
            {
                entities.Add(setupResolver[i]);
            }
        }

        var buffer = new EcsCommandBuffer(world);
        var rng = new Random(551514);
        for (var i = 0; i < 16; i++)
        {
            foreach (var entity in entities)
            {
                // Apply to 10% of entities
                if (rng.NextSingle() > 0.1f)
                {
                    continue;
                }

                // Do some random ops
                var count = rng.Next(1, 5);
                var update = rng.NextSingle() > 0.5;
                for (var j = 0; j < count; j++)
                {
                    switch (rng.Next(7))
                    {
                        case 0:
                            ChangeComponent<ComponentByte>(entity, buffer, update);
                            break;
                        case 1:
                            ChangeComponent<ComponentInt16>(entity, buffer, update);
                            break;
                        case 2:
                            ChangeComponent<ComponentFloat>(entity, buffer, update);
                            break;
                        case 3:
                            ChangeComponent<ComponentInt32>(entity, buffer, update);
                            break;
                        case 4:
                            ChangeComponent<ComponentInt64>(entity, buffer, update);
                            break;
                        case 5:
                            ChangeComponent<Component0>(entity, buffer, update);
                            break;
                        case 6:
                            ChangeComponent<Component1>(entity, buffer, update);
                            break;
                    }
                }
            }

            buffer.Execute().Dispose();
        }

        void ChangeComponent<T>(Entity e, EcsCommandBuffer b, bool update)
            where T : struct, IComponent
        {
            if (e.HasComponent<T>() && !update)
            {
                b.Remove<T>(e);
            }
            else
            {
                b.Set(e, default(T));
            }
        }
    }

    [TestMethod]
    public void CreateEntityLateResolveThrows()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var eb = buffer.Create();

        var resolver = buffer.Execute();
        resolver.Dispose();

        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            eb.Resolve();
        });
    }

    [TestMethod]
    public void CreateEntityCannotResolveFromAnotherBuffer()
    {
        var world = new EcsWorld();
        var buffer1 = new EcsCommandBuffer(world);
        var buffer2 = new EcsCommandBuffer(world);

        // Create the entity
        var eb1 = buffer1.Create();
        buffer1.Execute().Dispose();

        // Also run the other buffer to get another resolver
        using var resolver2 = buffer2.Execute();

        // Resolve the entity ID using the wrong resolver
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            eb1.Resolve();
        });
    }

    [TestMethod]
    public void CreateEntityCannotResolveFromPreviousPlayback()
    {
        var world = new EcsWorld();
        var buffer1 = new EcsCommandBuffer(world);

        // Create the entity
        var eb1 = buffer1.Create();
        buffer1.Execute().Dispose();

        // Re-use that buffer
        using var resolver2 = buffer1.Execute();

        // Resolve the entity ID using the wrong resolver
        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            eb1.Resolve();
        });
    }

    [TestMethod]
    public void ModifyBufferedEntityAfterPlaybackThrows()
    {
        var world = new EcsWorld();
        var buffer1 = new EcsCommandBuffer(world);

        // Create the entity
        var eb1 = buffer1.Create();
        using var resolver = buffer1.Execute();

        // Try to modify the buffered entity
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            eb1.Set(new ComponentFloat(8));
        });
    }

    [TestMethod]
    public void CreateEntityAndSet()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var eb = buffer
                .Create()
                .Set(new ComponentFloat(17));

        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        Assert.IsTrue(entity.IsAlive);
        Assert.AreEqual(1, world.Archetypes.Count);
        Assert.AreEqual(1, world.Archetypes.Single().Components.Count);
        Assert.IsTrue(world.Archetypes.Single().Components.Contains(ComponentId.Get<ComponentFloat>()));
    }

    [TestMethod]
    public void SetTwiceOnNewEntityWithOverwrite()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var eb = buffer.Create();

        eb.Set(new ComponentFloat(1));
        eb.Set(new ComponentFloat(2));
    }

    [TestMethod]
    public void DestroyEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var buffered = new[]
        {
            buffer.Create().Set(new ComponentFloat(1)),
            buffer.Create().Set(new ComponentFloat(2)),
            buffer.Create().Set(new ComponentFloat(3)),
        };

        using var resolver = buffer.Execute();

        var entities = new[]
        {
            buffered[0].Resolve(),
            buffered[1].Resolve(),
            buffered[2].Resolve(),
        };

        foreach (var entity in entities)
        {
            Assert.IsTrue(entity.IsAlive);
        }

        buffer.Destroy(entities[1]);
        buffer.Execute();

        Assert.IsTrue(entities[0].IsAlive);
        Assert.IsFalse(entities[1].IsAlive);
        Assert.IsTrue(entities[2].IsAlive);

        Assert.AreEqual(1, entities[0].GetComponent<ComponentFloat>().Value);
        Assert.AreEqual(3, entities[2].GetComponent<ComponentFloat>().Value);
    }

    [TestMethod]
    public void DestroyEntityTwice()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var buffered = buffer.Create().Set(new ComponentFloat(1));
        using var resolver = buffer.Execute();
        var entity = buffered.Resolve();
        Assert.IsTrue(entity.IsAlive);

        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Execute();

        Assert.IsFalse(entity.IsAlive);
    }

    [TestMethod]
    public void DestroyDeadEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity
        var buffered = buffer.Create().Set(new ComponentFloat(1));
        using var resolver = buffer.Execute();
        var entity = buffered.Resolve();
        Assert.IsTrue(entity.IsAlive);

        // Setup deletion for that entity
        buffer.Destroy(entity);

        // Destroy that entity before playing back the first buffer
        var buffer2 = new EcsCommandBuffer(world);
        buffer2.Destroy(entity);
        buffer2.Execute().Dispose();
        Assert.IsFalse(entity.IsAlive);

        // Now play the first buffer back
        buffer.Execute().Dispose();

        Assert.IsFalse(entity.IsAlive);
    }

    [TestMethod]
    public void DestroyEntities()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        var buffered = new[]
        {
            buffer.Create().Set(new ComponentFloat(1)),
            buffer.Create().Set(new ComponentFloat(2)),
            buffer.Create().Set(new ComponentFloat(3)),
        };

        using var resolver = buffer.Execute();

        var entities = new[]
        {
            buffered[0].Resolve(),
            buffered[1].Resolve(),
            buffered[2].Resolve(),
        };

        foreach (var entity in entities)
        {
            Assert.IsTrue(entity.IsAlive);
        }

        buffer.Destroy([entities[0], entities[1]]);
        buffer.Execute();

        Assert.IsFalse(entities[0].IsAlive);
        Assert.IsFalse(entities[1].IsAlive);
        Assert.IsTrue(entities[2].IsAlive);

        Assert.AreEqual(3, entities[2].GetComponent<ComponentFloat>().Value);
    }

    [TestMethod]
    public void DestroyByQueryMixedWorlds()
    {
        var world1 = new EcsWorld();
        var world2 = new EcsWorld();

        var cmd = new EcsCommandBuffer(world1);

        var q = new QueryBuilder().Include<Component0>().Build(world2);

        Assert.ThrowsException<GuardException>(() =>
        {
            cmd.Destroy(q);
        });
    }

    [TestMethod]
    public void DestroyByQuery()
    {
        var world = new EcsWorld();

        TestHelpers.SetupRandomEntities(world, count: 100000).Execute().Dispose();

        // Get the archetypes we're about to destroy
        var deleting = (from archetype in world.Archetypes
                        where archetype.Components.Contains(ComponentId.Get<Component0>())
                        where archetype.Components.Contains(ComponentId.Get<Component1>())
                        where !archetype.Components.Contains(ComponentId.Get<ComponentInt32>())
                        select archetype).ToArray();

        // Get all other archetypes
        var others = (from archetype in world.Archetypes
                      where !deleting.Contains(archetype)
                      select (archetype, archetype.EntityCount)).ToArray();

        // Query to destroy
        var q = new QueryBuilder().Include<Component0>().Include<Component1>().Exclude<ComponentInt32>().Build(world);

        // Get an entity we're going to destroy
        var dead = q.FirstOrDefault()!.Value;

        // Destroy it
        var buffer = new EcsCommandBuffer(world);
        buffer.Destroy(q);
        buffer.Execute().Dispose();

        // Check the archetypes
        foreach (var archetype in deleting)
        {
            Assert.AreEqual(0, archetype.EntityCount);
        }

        foreach (var (archetype, count) in others)
        {
            Assert.AreEqual(count, archetype.EntityCount);
        }

        // Check it's dead
        Assert.IsFalse(dead.IsAlive);
    }

    [TestMethod]
    public void ModifyThenDestroy()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create entity
        var buffered = buffer.Create().Set(new ComponentFloat(1));
        using var resolver = buffer.Execute();
        var entity = buffered.Resolve();
        Assert.IsTrue(entity.IsAlive);

        // Modify it _and_ destroy it
        buffer.Set(entity, new ComponentInt64(7));
        buffer.Remove<ComponentFloat>(entity);
        buffer.Destroy(entity);

        buffer.Execute().Dispose();

        // Check it's dead
        Assert.IsFalse(entity.IsAlive);
        Assert.AreEqual(0, new QueryBuilder().Include<ComponentFloat>().Build(world).Count());
        Assert.AreEqual(0, new QueryBuilder().Include<ComponentInt16>().Build(world).Count());
        Assert.AreEqual(0, new QueryBuilder().Include<ComponentInt32>().Build(world).Count());
        Assert.AreEqual(0, new QueryBuilder().Include<ComponentInt64>().Build(world).Count());
        Assert.AreEqual(0, new QueryBuilder().Include<ComponentFloat>().Build(world).Count());
    }

    [TestMethod]
    public void RemoveFromEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<ComponentInt16>(entity);
        buffer.Execute();

        Assert.AreEqual(123, entity.GetComponent<ComponentFloat>().Value);
        Assert.IsFalse(entity.HasComponent<ComponentInt16>());
    }

    [TestMethod]
    public void AddToEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Add a third
        buffer.Set(entity, new ComponentInt32(789));
        buffer.Execute();

        // Check they are all present
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsTrue(entity.HasComponent<ComponentInt16>());
        Assert.IsTrue(entity.HasComponent<ComponentInt32>());
    }

    [TestMethod]
    public void SetOnEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one
        buffer.Set(entity, new ComponentInt16(789));
        buffer.Execute();

        // Check the value has changed
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsTrue(entity.HasComponent<ComponentInt16>());
        Assert.AreEqual(789, entity.GetComponent<ComponentInt16>().Value);
    }

    [TestMethod]
    public void SetTwiceOnEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one, twice
        buffer.Set(entity, new ComponentInt16(789));
        buffer.Set(entity, new ComponentInt16(987));
        buffer.Execute();

        // Check the value has changed to the latest value
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsTrue(entity.HasComponent<ComponentInt16>());
        Assert.AreEqual(987, entity.GetComponent<ComponentInt16>().Value);
    }

    [TestMethod]
    public void SetThenRemoveOnEntity()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one
        buffer.Set(entity, new ComponentInt16(789));

        // Then remove it
        buffer.Remove<ComponentInt16>(entity);

        buffer.Execute();

        // Check the value is gone
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsFalse(entity.HasComponent<ComponentInt16>());
    }

    [TestMethod]
    public void RemoveInvalidComponent()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<ComponentInt32>(entity);

        buffer.Execute();

        // Check entity is unchanged
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsTrue(entity.HasComponent<ComponentInt16>());
        Assert.IsFalse(entity.HasComponent<ComponentInt32>());
    }

    [TestMethod]
    public void RemoveAndSetComponent()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create an entity with 2 components
        var eb = buffer
                .Create()
                .Set(new ComponentFloat(123))
                .Set(new ComponentInt16(456));
        using var resolver = buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<ComponentInt16>(entity);

        // Then set the same component!
        buffer.Set(entity, new ComponentInt16(789));

        buffer.Execute();

        // Check entity structure is unchanged
        Assert.IsTrue(entity.HasComponent<ComponentFloat>());
        Assert.IsTrue(entity.HasComponent<ComponentInt16>());
        Assert.IsFalse(entity.HasComponent<ComponentInt32>());

        // Check value is correct
        Assert.AreEqual(789, entity.GetComponent<ComponentInt16>().Value);
    }

    [TestMethod]
    public void CreateManyArchetypes()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create entities in lots of different archetypes. The idea is to
        // create so many the entity runs out of aggregation buffers.

        var buffered = new List<BufferedEntity>();
        var rng = new Random(17);
        for (var i = 0; i < 1024; i++)
        {
            var eb = buffer.Create();
            buffered.Add(eb);

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: eb.Set(new Component0()); break;
                    case 1: eb.Set(new Component1()); break;
                    case 2: eb.Set(new Component2()); break;
                    case 3: eb.Set(new Component3()); break;
                    case 4: eb.Set(new Component4()); break;
                    case 5: eb.Set(new Component5()); break;
                    case 6: eb.Set(new Component6()); break;
                    case 7: eb.Set(new Component7()); break;
                    case 8: eb.Set(new Component8()); break;
                    case 9: eb.Set(new Component9()); break;
                    case 10: eb.Set(new Component10()); break;
                    case 11: eb.Set(new Component11()); break;
                    case 12: eb.Set(new Component12()); break;
                    case 13: eb.Set(new Component13()); break;
                    case 14: eb.Set(new Component14()); break;
                    case 15: eb.Set(new Component15()); break;
                    case 16: eb.Set(new Component16()); break;
                    case 17: eb.Set(new Component17()); break;
                }
            }
        }

        using var resolver = buffer.Execute();
        Assert.AreEqual(1024, resolver.Count);

        // Ensure this is identical to the loop above!
        rng = new Random(17);
        for (var i = 0; i < buffered.Count; i++)
        {
            var entity = buffered[i].Resolve();

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: Assert.IsTrue(entity.HasComponent<Component0>()); break;
                    case 1: Assert.IsTrue(entity.HasComponent<Component1>()); break;
                    case 2: Assert.IsTrue(entity.HasComponent<Component2>()); break;
                    case 3: Assert.IsTrue(entity.HasComponent<Component3>()); break;
                    case 4: Assert.IsTrue(entity.HasComponent<Component4>()); break;
                    case 5: Assert.IsTrue(entity.HasComponent<Component5>()); break;
                    case 6: Assert.IsTrue(entity.HasComponent<Component6>()); break;
                    case 7: Assert.IsTrue(entity.HasComponent<Component7>()); break;
                    case 8: Assert.IsTrue(entity.HasComponent<Component8>()); break;
                    case 9: Assert.IsTrue(entity.HasComponent<Component9>()); break;
                    case 10: Assert.IsTrue(entity.HasComponent<Component10>()); break;
                    case 11: Assert.IsTrue(entity.HasComponent<Component11>()); break;
                    case 12: Assert.IsTrue(entity.HasComponent<Component12>()); break;
                    case 13: Assert.IsTrue(entity.HasComponent<Component13>()); break;
                    case 14: Assert.IsTrue(entity.HasComponent<Component14>()); break;
                    case 15: Assert.IsTrue(entity.HasComponent<Component15>()); break;
                    case 16: Assert.IsTrue(entity.HasComponent<Component16>()); break;
                    case 17: Assert.IsTrue(entity.HasComponent<Component17>()); break;
                }
            }
        }
    }

    [TestMethod]
    public void StructuralChanges()
    {
        var world = new EcsWorld();
        var buffer = new EcsCommandBuffer(world);

        // Create some entities
        buffer.Create().Set(new Component0()).Set(new Component1()).Set(new Component2());
        buffer.Create().Set(new Component0()).Set(new Component1()).Set(new Component2());
        buffer.Create().Set(new Component0()).Set(new Component1()).Set(new Component2());
        buffer.Create().Set(new Component0()).Set(new Component1()).Set(new Component2());
        var resolver = buffer.Execute();
        var entity0 = resolver[0];
        var entity1 = resolver[1];
        var entity2 = resolver[2];
        var entity3 = resolver[3];
        resolver.Dispose();

        // Add component to 0
        buffer.Set(entity0, new Component3());

        // Remove component from 1
        buffer.Remove<Component0>(entity1);

        // Add and remove 2
        buffer.Remove<Component0>(entity2);
        buffer.Set(entity2, new Component4());

        // Do nothing to 3

        // Apply all that
        buffer.Execute().Dispose();

        // Check 1 has everything expected
        Assert.AreEqual(4, entity0.ComponentIds.Count);
        entity0.GetComponent<Component0>();
        entity0.GetComponent<Component1>();
        entity0.GetComponent<Component2>();
        entity0.GetComponent<Component3>();

        // Check 2 has everything expected
        Assert.AreEqual(2, entity1.ComponentIds.Count);
        entity1.GetComponent<Component1>();
        entity1.GetComponent<Component2>();

        // Check 3 has everything expected
        Assert.AreEqual(3, entity2.ComponentIds.Count);
        entity2.GetComponent<Component1>();
        entity2.GetComponent<Component2>();
        entity2.GetComponent<Component4>();

        // Check other is unchanged
        Assert.AreEqual(3, entity3.ComponentIds.Count);
        entity3.GetComponent<Component0>();
        entity3.GetComponent<Component1>();
        entity3.GetComponent<Component2>();
    }

    [TestMethod]
    public void ClearSet()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0());
        var r = cmd.Execute();
        var e = eb.Resolve();
        r.Dispose();

        // Set value, then clear buffer
        cmd.Set(e, new Component1());
        cmd.Clear();
        cmd.Execute().Dispose();

        Assert.IsTrue(e.HasComponent<Component0>());
        Assert.IsFalse(e.HasComponent<Component1>());
    }

    [TestMethod]
    public void ClearBufferedSet()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0());
        var r = cmd.Execute();
        var e = eb.Resolve();
        r.Dispose();

        // Create another entity then clear
        cmd.Create().Set(new Component0());
        cmd.Clear();
        cmd.Execute().Dispose();

        Assert.AreEqual(1, new QueryBuilder().Include<Component0>().Build(world).Count());
    }

    [TestMethod]
    public void ResolveClearedEntity()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0());
        cmd.Clear();
        cmd.Execute();

        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            eb.Resolve();
        });
    }


    [TestMethod]
    public void ClearBufferRemove()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0()).Set(new Component1());
        var r = cmd.Execute();
        var e = eb.Resolve();
        r.Dispose();

        // Remove value, then clear buffer
        cmd.Remove<Component1>(e);
        cmd.Clear();
        cmd.Execute().Dispose();

        Assert.IsTrue(e.HasComponent<Component0>());
        Assert.IsTrue(e.HasComponent<Component1>());
    }

    [TestMethod]
    public void ClearBufferDestroy()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0()).Set(new Component1());
        var r = cmd.Execute();
        var e = eb.Resolve();
        r.Dispose();

        // Destroy entity, then clear buffer
        cmd.Destroy(e);
        cmd.Clear();
        cmd.Execute().Dispose();

        Assert.IsTrue(e.IsAlive);
        Assert.IsTrue(e.HasComponent<Component0>());
        Assert.IsTrue(e.HasComponent<Component1>());
    }

    [TestMethod]
    public void ClearBufferDestroyArchetype()
    {
        var world = new EcsWorld();
        var cmd = new EcsCommandBuffer(world);

        // Create an entity
        var eb = cmd.Create().Set(new Component0()).Set(new Component1());
        var r = cmd.Execute();
        var e = eb.Resolve();
        r.Dispose();

        // Destroy archetypes, then clear buffer
        cmd.Destroy(new QueryBuilder().Include<Component0>().Build(world));
        cmd.Clear();
        cmd.Execute().Dispose();

        Assert.IsTrue(e.IsAlive);
        Assert.IsTrue(e.HasComponent<Component0>());
        Assert.IsTrue(e.HasComponent<Component1>());
    }
}
