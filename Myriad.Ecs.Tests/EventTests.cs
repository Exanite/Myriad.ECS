using Exanite.Core.Events;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class EventTests
{
    [TestMethod]
    public void CreateEntity_RaisesEntityCreatedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityCreatedCount);
    }

    [TestMethod]
    public void DestroyEntity_UsingEntities_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Destroy(entity);
                }
            }
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityDestroyedCount);
    }

    [TestMethod]
    public void DestroyEntity_UsingQuery_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityDestroyedCount);
    }

    [TestMethod]
    public void DestroyEntity_UsingEntities_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute().Dispose();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Destroy(entity);
                }
            }
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentRemovedCount);
    }

    [TestMethod]
    public void DestroyEntity_UsingQuery_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute().Dispose();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentRemovedCount);
    }

    [TestMethod]
    public void RemoveComponent_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute().Dispose();

        // Remove components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Remove<Component0>(entity);
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentRemovedCount);
    }

    [TestMethod]
    public void SetComponent_Once_OnBufferedEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);
    }

    [TestMethod]
    public void SetComponent_Once_OnWorldEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);
    }

    [TestMethod]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnBufferedEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentModifiedCount);
    }

    [TestMethod]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnWorldEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);

        // Set components again
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentModifiedCount);
    }

    // TODO: Consider changing behavior
    [TestMethod]
    public void SetComponent_Twice_InSameCommandBuffer_OnBufferedEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Component0())
                .Set(new Component0());
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);
        Assert.AreEqual(0, handler.ComponentModifiedCount);
    }

    // TODO: Consider changing behavior
    [TestMethod]
    public void SetComponent_Twice_InSameCommandBuffer_OnWorldEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer
                        .Set(entity, new Component0())
                        .Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.ComponentAddedCount);
        Assert.AreEqual(0, handler.ComponentModifiedCount);
    }

    [TestMethod]
    public void DestroyEntity_AfterModifyingEntity_InSameCommandBuffer_OnlyRaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();

        // Modify entity and destroy entity
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                    commandBuffer.Set(entity, new Component0());

                    commandBuffer.Destroy(entity);
                }
            }
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(0, handler.ComponentAddedCount);
        Assert.AreEqual(0, handler.ComponentModifiedCount);
        Assert.AreEqual(entityAddCount, handler.EntityDestroyedCount);
    }

    [TestMethod]
    public void DisposeWorld_DestroysAllEntities_And_RaisesComponentRemovedAndEntityDestroyedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterSendAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Component0());
        }

        commandBuffer.Execute().Dispose();

        // Dispose world
        world.Dispose();

        Assert.AreEqual(entityAddCount, handler.EntityDestroyedCount);
        Assert.AreEqual(entityAddCount, handler.ComponentRemovedCount);
    }

    [TestMethod]
    public void CommandBufferExecute_AfterDisposingWorld_ThrowsException()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        world.Dispose();
        Assert.ThrowsException<GuardException>(() => commandBuffer.Execute());
    }

    private class WorldEventHandler :
        IEventHandler<EntityCreatedEvent>,
        IEventHandler<EntityDestroyedEvent>,
        IEventHandler<ComponentAdded<Component0>>,
        IEventHandler<ComponentModified<Component0>>,
        IEventHandler<ComponentRemoved<Component0>>
    {
        public int EntityCreatedCount { get; private set; }
        public int EntityDestroyedCount { get; private set; }

        public int ComponentAddedCount { get; private set; }
        public int ComponentModifiedCount { get; private set; }
        public int ComponentRemovedCount { get; private set; }

        public WorldEventHandler RegisterAll(EcsWorld world)
        {
            world.EventBus.Register<EntityCreatedEvent>(this);
            world.EventBus.Register<EntityDestroyedEvent>(this);
            world.EventBus.Register<ComponentAdded<Component0>>(this);
            world.EventBus.Register<ComponentModified<Component0>>(this);
            world.EventBus.Register<ComponentRemoved<Component0>>(this);

            return this;
        }

        public void OnEvent(EntityCreatedEvent e)
        {
            EntityCreatedCount++;
        }

        public void OnEvent(EntityDestroyedEvent e)
        {
            EntityDestroyedCount++;
        }

        public void OnEvent(ComponentAdded<Component0> e)
        {
            ComponentAddedCount++;
        }

        public void OnEvent(ComponentModified<Component0> e)
        {
            ComponentModifiedCount++;
        }

        public void OnEvent(ComponentRemoved<Component0> e)
        {
            ComponentRemovedCount++;
        }
    }

    private class EventLogger : IAllEventHandler
    {
        void IAllEventHandler.OnEvent<T>(T e)
        {
            typeof(T).Dump("Event type");
        }
    }
}
