using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EcsCommandBufferTests
{
    [Fact]
    public void CreateCommandBuffer()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();
        Assert.NotNull(commandBuffer);
    }

    [Fact]
    public void CreateEntity()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var entity = commandBuffer.Create().Entity;
        commandBuffer.Execute();

        Assert.True(entity.IsAlive);
        Assert.Equal(1, world.ArchetypesList.Count);
        Assert.Equal(0, world.ArchetypesList.Single().Components.Count);
    }

    [Fact]
    public void CreateManyEntities()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create lots of entities
        var entities = new List<Entity>();
        for (var i = 0; i < 50000; i++)
        {
            entities.Add(commandBuffer.Create().Set(new EcsInt32(i)).Entity);
        }

        // Execute buffer
        commandBuffer.Execute();

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            Assert.True(entity.IsAlive);
            Assert.Equal(1, world.ArchetypesList.Count);
            Assert.Equal(1, world.ArchetypesList.Single().Components.Count);
            Assert.Equal(i, entity.Get<EcsInt32>().Value);
        }
    }

    [Fact]
    public void ChurnCreateDestroy()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var rng = new Random(46576);

        // Keep track of every single entity ever created
        var alive = new List<Entity>();
        var dead = new List<Entity>();

        // Do lots of rounds of creation and destruction
        for (var i = 0; i < 20; i++)
        {
            // Create lots of entities
            var newEntities = new List<Entity>();
            for (var j = 0; j < 10000; j++)
            {
                var b = buffer.Create().Set(new EcsInt32(j));
                newEntities.Add(b);

                for (var k = 0; k < 3; k++)
                {
                    switch (rng.Next(0, 6))
                    {
                        case 0: b.Set(new EcsByte((byte)i)); break;
                        case 1: b.Set(new EcsInt16((short)i)); break;
                        case 2: b.Set(new EcsFloat(i)); break;
                        case 3: b.Set(new EcsInt32(i)); break;
                        case 4: b.Set(new EcsInt64(i)); break;
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
                Assert.True(ent.IsAlive);
                buffer.Destroy(ent);
                alive.RemoveAt(index);
                dead.Add(ent);
            }

            // Execute
            buffer.Execute();

            // Add new entities to alive list
            foreach (var entity in newEntities)
            {
                alive.Add(entity);
            }

            // Check all the entities
            foreach (var entity in alive)
            {
                Assert.True(entity.IsAlive);
            }

            foreach (var entity in dead)
            {
                Assert.True(!entity.IsAlive);
            }

            // Check archetypes
            Assert.Equal(alive.Count, world.ArchetypesList.Select(a => a.EntityCount).Sum());
        }
    }

    [Fact]
    public void ChurnStructural()
    {
        var world = new EcsWorld();
        var entities = TestHelpers.AddRandomEntities(world, 10_000);

        var commandBuffer = world.AcquireCommandBuffer();
        var random = new Random(551514);
        for (var i = 0; i < 16; i++)
        {
            foreach (var entity in entities)
            {
                // Apply to 10% of entities
                if (random.NextSingle() > 0.1f)
                {
                    continue;
                }

                // Do some random ops
                var count = random.Next(1, 5);
                var update = random.NextSingle() > 0.5;
                for (var j = 0; j < count; j++)
                {
                    switch (random.Next(7))
                    {
                        case 0:
                            ChangeComponent<EcsByte>(entity, update);
                            break;
                        case 1:
                            ChangeComponent<EcsInt16>(entity, update);
                            break;
                        case 2:
                            ChangeComponent<EcsFloat>(entity, update);
                            break;
                        case 3:
                            ChangeComponent<EcsInt32>(entity, update);
                            break;
                        case 4:
                            ChangeComponent<EcsInt64>(entity, update);
                            break;
                        case 5:
                            ChangeComponent<Ecs0>(entity, update);
                            break;
                        case 6:
                            ChangeComponent<Ecs1>(entity, update);
                            break;
                    }
                }
            }

            commandBuffer.Execute();
        }

        return;

        void ChangeComponent<T>(Entity entity, bool update) where T : struct, IComponent
        {
            if (entity.Has<T>() && !update)
            {
                commandBuffer.Remove<T>(entity);
            }
            else
            {
                commandBuffer.Set(entity, default(T));
            }
        }
    }

    [Fact]
    public void CreateEntityAndSet()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var entity = buffer.Create().Set(new EcsFloat(17)).Entity;
        buffer.Execute();

        Assert.True(entity.IsAlive);
        Assert.Equal(1, world.ArchetypesList.Count);
        Assert.Equal(1, world.ArchetypesList.Single().Components.Count);
        Assert.True(world.ArchetypesList.Single().Components.Contains(ComponentId.Get<EcsFloat>()));
    }

    [Fact]
    public void SetTwiceOnNewEntityWithOverwrite()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var bufferedEntity = buffer.Create();

        bufferedEntity.Set(new EcsFloat(1));
        bufferedEntity.Set(new EcsFloat(2));
    }

    [Fact]
    public void DestroyEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var entities = new[]
        {
            buffer.Create().Set(new EcsFloat(1)).Entity,
            buffer.Create().Set(new EcsFloat(2)).Entity,
            buffer.Create().Set(new EcsFloat(3)).Entity,
        };

        buffer.Execute();

        foreach (var entity in entities)
        {
            Assert.True(entity.IsAlive);
        }

        buffer.Destroy(entities[1]);
        buffer.Execute();

        Assert.True(entities[0].IsAlive);
        Assert.False(entities[1].IsAlive);
        Assert.True(entities[2].IsAlive);

        Assert.Equal(1, entities[0].Get<EcsFloat>().Value);
        Assert.Equal(3, entities[2].Get<EcsFloat>().Value);
    }

    [Fact]
    public void DestroyEntityTwice()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var entity = buffer.Create().Set(new EcsFloat(1)).Entity;
        buffer.Execute();

        Assert.True(entity.IsAlive);

        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Execute();

        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void DestroyDeadEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity
        var entity = buffer.Create().Set(new EcsFloat(1)).Entity;
        buffer.Execute();

        Assert.True(entity.IsAlive);

        // Setup deletion for that entity
        buffer.Destroy(entity);

        // Destroy that entity before playing back the first buffer
        var buffer2 = world.AcquireCommandBuffer();
        buffer2.Destroy(entity);
        buffer2.Execute();
        Assert.False(entity.IsAlive);

        // Now play the first buffer back
        buffer.Execute();

        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void DestroyEntities()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var entities = new[]
        {
            buffer.Create().Set(new EcsFloat(1)).Entity,
            buffer.Create().Set(new EcsFloat(2)).Entity,
            buffer.Create().Set(new EcsFloat(3)).Entity,
        };

        buffer.Execute();

        foreach (var entity in entities)
        {
            Assert.True(entity.IsAlive);
        }

        buffer.Destroy([entities[0], entities[1]]);
        buffer.Execute();

        Assert.False(entities[0].IsAlive);
        Assert.False(entities[1].IsAlive);
        Assert.True(entities[2].IsAlive);

        Assert.Equal(3, entities[2].Get<EcsFloat>().Value);
    }

    [Fact]
    public void DestroyByQueryMixedWorlds()
    {
        var world1 = new EcsWorld();
        var world2 = new EcsWorld();

        // Implementation detail: The exception only gets triggered if the queries match something
        world1.AcquireCommandBuffer().Create().Set(new Ecs0()).CommandBuffer.Execute();
        world2.AcquireCommandBuffer().Create().Set(new Ecs0()).CommandBuffer.Execute();

        var commandBuffer = world1.AcquireCommandBuffer();
        var query = new QueryFilter().Include<Ecs0>().Build(world2);

        Assert.Throws<GuardException>(() =>
        {
            commandBuffer.Destroy(query);
        });
    }

    [Fact]
    public void DestroyByQuery()
    {
        var world = new EcsWorld();
        TestHelpers.AddRandomEntities(world, count: 100000);

        // Get the archetypes we're about to destroy
        var deleting = (from archetype in world.ArchetypesList
            where archetype.Components.Contains(ComponentId.Get<Ecs0>())
            where archetype.Components.Contains(ComponentId.Get<Ecs1>())
            where !archetype.Components.Contains(ComponentId.Get<EcsInt32>())
            select archetype).ToArray();

        // Get all other archetypes
        var others = (from archetype in world.ArchetypesList
            where !deleting.Contains(archetype)
            select (archetype, archetype.EntityCount)).ToArray();

        // Query to destroy
        var q = new QueryFilter().Include<Ecs0>().Include<Ecs1>().Exclude<EcsInt32>().Build(world);

        // Get an entity we're going to destroy
        var dead = q.FirstOrDefault();

        // Destroy it
        var buffer = world.AcquireCommandBuffer();
        buffer.Destroy(q);
        buffer.Execute();

        // Check the archetypes
        foreach (var archetype in deleting)
        {
            Assert.Equal(0, archetype.EntityCount);
        }

        foreach (var (archetype, count) in others)
        {
            Assert.Equal(count, archetype.EntityCount);
        }

        // Check it's dead
        Assert.False(dead.IsAlive);
    }

    [Fact]
    public void ModifyThenDestroy()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create entity
        var entity = buffer.Create().Set(new EcsFloat(1)).Entity;
        buffer.Execute();

        Assert.True(entity.IsAlive);

        // Modify it _and_ destroy it
        buffer.Set(entity, new EcsInt64(7));
        buffer.Remove<EcsFloat>(entity);
        buffer.Destroy(entity);

        buffer.Execute();

        // Check it's dead
        Assert.False(entity.IsAlive);
        Assert.Equal(0, new QueryFilter().Include<EcsFloat>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt16>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt32>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt64>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsFloat>().Build(world).Count());
    }

    [Fact]
    public void RemoveFromEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        buffer.Execute();

        // Remove a component
        buffer.Remove<EcsInt16>(entity);
        buffer.Execute();

        Assert.Equal(123, entity.Get<EcsFloat>().Value);
        Assert.False(entity.Has<EcsInt16>());
    }

    [Fact]
    public void AddToEntity()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = commandBuffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        commandBuffer.Execute();

        // Add a third
        commandBuffer.Set(entity, new EcsInt32(789));
        commandBuffer.Execute();

        // Check they are all present
        Assert.True(entity.Has<EcsFloat>());
        Assert.True(entity.Has<EcsInt16>());
        Assert.True(entity.Has<EcsInt32>());
    }

    [Fact]
    public void SetOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        buffer.Execute();

        // Overwrite one
        buffer.Set(entity, new EcsInt16(789));
        buffer.Execute();

        // Check the value has changed
        Assert.True(entity.Has<EcsFloat>());
        Assert.True(entity.Has<EcsInt16>());
        Assert.Equal(789, entity.Get<EcsInt16>().Value);
    }

    [Fact]
    public void SetBoxed()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create()
            .SetBoxed(new EcsFloat(123))
            .SetBoxed(new EcsInt16(456))
            .Entity;

        buffer.Execute();

        // Check the value has changed
        Assert.True(entity.Has<EcsFloat>());
        Assert.True(entity.Has<EcsInt16>());
        Assert.Equal(456, entity.Get<EcsInt16>().Value);
    }

    [Fact]
    public void SetTwiceOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        buffer.Execute();

        // Overwrite one, twice
        buffer.Set(entity, new EcsInt16(789));
        buffer.Set(entity, new EcsInt16(987));
        buffer.Execute();

        // Check the value has changed to the latest value
        Assert.True(entity.Has<EcsFloat>());
        Assert.True(entity.Has<EcsInt16>());
        Assert.Equal(987, entity.Get<EcsInt16>().Value);
    }

    [Fact]
    public void SetThenRemoveOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        buffer.Execute();

        // Overwrite one
        buffer.Set(entity, new EcsInt16(789));

        // Then remove it
        buffer.Remove<EcsInt16>(entity);
        buffer.Execute();

        // Check the value is gone
        Assert.True(entity.Has<EcsFloat>());
        Assert.False(entity.Has<EcsInt16>());
    }

    [Fact]
    public void RemoveInvalidComponent()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Entity;
        buffer.Execute();

        // Remove an invalid component
        buffer.Remove<Ecs2>(entity);
        buffer.Execute();

        // Check entity is unchanged
        Assert.True(entity.Has<Ecs0>());
        Assert.True(entity.Has<Ecs1>());
        Assert.False(entity.Has<Ecs2>());
    }

    /// <summary>
    /// This catches a regression that <see cref="RemoveInvalidComponent"/> does not catch.
    /// </summary>
    [Fact]
    public void RemoveInvalidComponent_WithStructuralChange()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Entity;
        buffer.Execute();

        // Remove a component
        buffer.Remove<Ecs1>(entity);

        // Remove an invalid component
        buffer.Remove<Ecs2>(entity);
        buffer.Execute();

        // Check entity is modified as expected
        Assert.True(entity.Has<Ecs0>());
        Assert.False(entity.Has<Ecs1>());
        Assert.False(entity.Has<Ecs2>());
    }

    [Fact]
    public void RemoveAndSetComponent()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var entity = buffer.Create().Set(new EcsFloat(123)).Set(new EcsInt16(456)).Entity;
        buffer.Execute();

        // Remove a component
        buffer.Remove<EcsInt16>(entity);

        // Then set the same component!
        buffer.Set(entity, new EcsInt16(789));
        buffer.Execute();

        // Check entity structure is unchanged
        Assert.True(entity.Has<EcsFloat>());
        Assert.True(entity.Has<EcsInt16>());
        Assert.False(entity.Has<EcsInt32>());

        // Check value is correct
        Assert.Equal(789, entity.Get<EcsInt16>().Value);
    }

    [Fact]
    public void CreateManyArchetypes()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create entities in lots of different archetypes. The idea is to
        // create so many the entity runs out of aggregation buffers.

        var entities = new List<Entity>();
        var rng = new Random(17);
        for (var i = 0; i < 1024; i++)
        {
            var entity = commandBuffer.Create();
            entities.Add(entity);

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: entity.Set(new Ecs0()); break;
                    case 1: entity.Set(new Ecs1()); break;
                    case 2: entity.Set(new Ecs2()); break;
                    case 3: entity.Set(new Ecs3()); break;
                    case 4: entity.Set(new Ecs4()); break;
                    case 5: entity.Set(new Ecs5()); break;
                    case 6: entity.Set(new Ecs6()); break;
                    case 7: entity.Set(new Ecs7()); break;
                    case 8: entity.Set(new Ecs8()); break;
                    case 9: entity.Set(new Ecs9()); break;
                    case 10: entity.Set(new Ecs10()); break;
                    case 11: entity.Set(new Ecs11()); break;
                    case 12: entity.Set(new Ecs12()); break;
                    case 13: entity.Set(new Ecs13()); break;
                    case 14: entity.Set(new Ecs14()); break;
                    case 15: entity.Set(new Ecs15()); break;
                    case 16: entity.Set(new Ecs16()); break;
                    case 17: entity.Set(new Ecs17()); break;
                }
            }
        }

        commandBuffer.Execute();

        // Ensure this is identical to the loop above!
        rng = new Random(17);
        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: Assert.True(entity.Has<Ecs0>()); break;
                    case 1: Assert.True(entity.Has<Ecs1>()); break;
                    case 2: Assert.True(entity.Has<Ecs2>()); break;
                    case 3: Assert.True(entity.Has<Ecs3>()); break;
                    case 4: Assert.True(entity.Has<Ecs4>()); break;
                    case 5: Assert.True(entity.Has<Ecs5>()); break;
                    case 6: Assert.True(entity.Has<Ecs6>()); break;
                    case 7: Assert.True(entity.Has<Ecs7>()); break;
                    case 8: Assert.True(entity.Has<Ecs8>()); break;
                    case 9: Assert.True(entity.Has<Ecs9>()); break;
                    case 10: Assert.True(entity.Has<Ecs10>()); break;
                    case 11: Assert.True(entity.Has<Ecs11>()); break;
                    case 12: Assert.True(entity.Has<Ecs12>()); break;
                    case 13: Assert.True(entity.Has<Ecs13>()); break;
                    case 14: Assert.True(entity.Has<Ecs14>()); break;
                    case 15: Assert.True(entity.Has<Ecs15>()); break;
                    case 16: Assert.True(entity.Has<Ecs16>()); break;
                    case 17: Assert.True(entity.Has<Ecs17>()); break;
                }
            }
        }
    }

    [Fact]
    public void StructuralChanges()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create some entities
        var entity0 = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2()).Entity;
        var entity1 = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2()).Entity;
        var entity2 = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2()).Entity;
        var entity3 = buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2()).Entity;
        buffer.Execute();

        // Add component to 0
        buffer.Set(entity0, new Ecs3());

        // Remove component from 1
        buffer.Remove<Ecs0>(entity1);

        // Add and remove 2
        buffer.Remove<Ecs0>(entity2);
        buffer.Set(entity2, new Ecs4());

        // Do nothing to 3

        // Apply all that
        buffer.Execute();

        // Check 1 has everything expected
        Assert.Equal(4, entity0.ComponentIds.Count);
        entity0.Get<Ecs0>();
        entity0.Get<Ecs1>();
        entity0.Get<Ecs2>();
        entity0.Get<Ecs3>();

        // Check 2 has everything expected
        Assert.Equal(2, entity1.ComponentIds.Count);
        entity1.Get<Ecs1>();
        entity1.Get<Ecs2>();

        // Check 3 has everything expected
        Assert.Equal(3, entity2.ComponentIds.Count);
        entity2.Get<Ecs1>();
        entity2.Get<Ecs2>();
        entity2.Get<Ecs4>();

        // Check other is unchanged
        Assert.Equal(3, entity3.ComponentIds.Count);
        entity3.Get<Ecs0>();
        entity3.Get<Ecs1>();
        entity3.Get<Ecs2>();
    }

    [Fact]
    public void ClearSet()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity
        var entity = commandBuffer.Create().Set(new Ecs0()).Entity;
        commandBuffer.Execute();

        // Set value, then clear buffer
        commandBuffer.Set(entity, new Ecs1());
        commandBuffer.Clear();
        commandBuffer.Execute();

        Assert.True(entity.Has<Ecs0>());
        Assert.False(entity.Has<Ecs1>());
    }

    [Fact]
    public void ClearBufferedSet()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        // Create another entity then clear
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Clear();
        commandBuffer.Execute();

        Assert.Equal(1, new QueryFilter().Include<Ecs0>().Build(world).Count());
    }

    [Fact]
    public void ClearBufferRemove()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity
        var entity = commandBuffer.Create().Set(new Ecs0()).Set(new Ecs1()).Entity;
        commandBuffer.Execute();

        // Remove value, then clear buffer
        commandBuffer.Remove<Ecs1>(entity);
        commandBuffer.Clear();
        commandBuffer.Execute();

        Assert.True(entity.Has<Ecs0>());
        Assert.True(entity.Has<Ecs1>());
    }

    [Fact]
    public void ClearBufferDestroy()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity
        var entity = commandBuffer.Create().Set(new Ecs0()).Set(new Ecs1()).Entity;
        commandBuffer.Execute();

        // Destroy entity, then clear buffer
        commandBuffer.Destroy(entity);
        commandBuffer.Clear();
        commandBuffer.Execute();

        Assert.True(entity.IsAlive);
        Assert.True(entity.Has<Ecs0>());
        Assert.True(entity.Has<Ecs1>());
    }

    [Fact]
    public void ClearBufferDestroyArchetype()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity
        var entity = commandBuffer.Create().Set(new Ecs0()).Set(new Ecs1()).Entity;
        commandBuffer.Execute();

        // Destroy archetypes, then clear buffer
        commandBuffer.Destroy(new QueryFilter().Include<Ecs0>().Build(world));
        commandBuffer.Clear();
        commandBuffer.Execute();

        Assert.True(entity.IsAlive);
        Assert.True(entity.Has<Ecs0>());
        Assert.True(entity.Has<Ecs1>());
    }
}
