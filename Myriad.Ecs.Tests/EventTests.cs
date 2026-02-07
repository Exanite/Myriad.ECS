using Exanite.Core.Events;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EventTests
{
    [Fact]
    public void World_CopyTo_RaisesComponentCopiedEvent()
    {
        var srcWorld = new EcsWorld();
        var srcHandler = new WorldEventHandler().RegisterAll(srcWorld);
        srcWorld.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            for (var i = 0; i < entityAddCount; i++)
            {
                commandBuffer.Create()
                    .Set(new Ecs0());
            }

            commandBuffer.Execute();
        }

        Assert.Equal(entityAddCount, srcHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, srcHandler.ComponentAddedCount);
        Assert.Equal(0, srcHandler.ComponentCopiedCount);

        // Copy to new world
        var dstWorld = new EcsWorld();
        var dstHandler = new WorldEventHandler().RegisterAll(dstWorld);
        dstWorld.EventBus.RegisterForwardAllTo(new EventLogger());

        srcWorld.CopyTo(dstWorld);

        Assert.Equal(entityAddCount, srcHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, srcHandler.ComponentAddedCount);
        Assert.Equal(0, srcHandler.ComponentCopiedCount);

        Assert.Equal(entityAddCount, dstHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, dstHandler.ComponentAddedCount);
        Assert.Equal(entityAddCount, dstHandler.ComponentCopiedCount);
    }

    [Fact]
    public void Entity_CopyFrom_RaisesComponentCopiedEvent()
    {
        var srcWorld = new EcsWorld();
        var srcHandler = new WorldEventHandler().RegisterAll(srcWorld);
        srcWorld.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            for (var i = 0; i < entityAddCount; i++)
            {
                commandBuffer.Create()
                    .Set(new Ecs0());
            }

            commandBuffer.Execute();
        }

        Assert.Equal(entityAddCount, srcHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, srcHandler.ComponentAddedCount);
        Assert.Equal(0, srcHandler.ComponentCopiedCount);

        // Copy to new world
        var dstWorld = new EcsWorld();
        var dstHandler = new WorldEventHandler().RegisterAll(dstWorld);
        dstWorld.EventBus.RegisterForwardAllTo(new EventLogger());

        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            foreach (var entity in srcWorld.EnumerateEntities())
            {
                commandBuffer.Create().CopyFrom(entity);
            }

            commandBuffer.Execute();
        }

        Assert.Equal(entityAddCount, srcHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, srcHandler.ComponentAddedCount);
        Assert.Equal(0, srcHandler.ComponentCopiedCount);

        Assert.Equal(entityAddCount, dstHandler.EntityCreatedCount);
        Assert.Equal(entityAddCount, dstHandler.ComponentAddedCount);
        Assert.Equal(entityAddCount, dstHandler.ComponentCopiedCount);
    }

    [Fact]
    public void CreateEntity_RaisesEntityCreatedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityCreatedCount);
    }

    [Fact]
    public void DestroyEntity_UsingEntities_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Destroy(entity);
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DestroyEntity_UsingQuery_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryFilter().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DestroyEntity_UsingEntities_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Ecs0());
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Destroy(entity);
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void DestroyEntity_UsingQuery_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Ecs0());
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryFilter().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void RemoveComponent_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Ecs0());
        }

        commandBuffer.Execute();

        // Remove components
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Remove<Ecs0>(entity);
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void SetComponent_Once_OnNewEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
    }

    [Fact]
    public void SetComponent_Once_OnExistingEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Set(entity, new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
    }

    [Fact]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnNewEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);

        // Set components
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Set(entity, new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnExistingEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Set(entity, new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);

        // Set components again
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Set(entity, new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InSameCommandBuffer_OnBufferedEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Ecs0())
                .Set(new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InSameCommandBuffer_OnWorldEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer
                .Use(entity)
                .Set(new Ecs0())
                .Set(new Ecs0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
    }

    [Fact]
    public void DestroyEntity_AfterModifyingEntity_InSameCommandBuffer_OnlyRaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Modify entity and destroy entity
        var allEntitiesQuery = new QueryFilter().Build(world);
        foreach (var entity in allEntitiesQuery.EnumerateEntities())
        {
            commandBuffer.Use(entity)
                .Set(new Ecs0())
                .Set(new Ecs1());

            commandBuffer.Destroy(entity);
        }

        commandBuffer.Execute();
        Assert.Equal(0, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DisposeWorld_DestroysAllEntities_And_RaisesComponentRemovedAndEntityDestroyedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Ecs0());
        }

        commandBuffer.Execute();

        // Dispose world
        world.Dispose();

        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void CommandBufferExecute_AfterDisposingWorld_ThrowsException()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        world.Dispose();
        Assert.Throws<GuardException>(() => commandBuffer.Execute());
    }

    [Fact]
    public void DisposingWorld_ClearsCommandBuffer()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        world.Dispose();
        Assert.False(commandBuffer.HasBufferedOperations);
    }

    private class WorldEventHandler :
        IEventHandler<EntityCreatedEvent>,
        IEventHandler<EntityDestroyedEvent>,
        IEventHandler<ComponentCopiedEvent<Ecs0>>,
        IEventHandler<ComponentAddedEvent<Ecs0>>,
        IEventHandler<ComponentModifiedEvent<Ecs0>>,
        IEventHandler<ComponentRemovedEvent<Ecs0>>
    {
        public int EntityCreatedCount { get; private set; }
        public int EntityDestroyedCount { get; private set; }

        public int ComponentCopiedCount { get; private set; }
        public int ComponentAddedCount { get; private set; }
        public int ComponentModifiedCount { get; private set; }
        public int ComponentRemovedCount { get; private set; }

        public WorldEventHandler RegisterAll(EcsWorld world)
        {
            world.EventBus.Register<EntityCreatedEvent>(this);
            world.EventBus.Register<EntityDestroyedEvent>(this);
            world.EventBus.Register<ComponentCopiedEvent<Ecs0>>(this);
            world.EventBus.Register<ComponentAddedEvent<Ecs0>>(this);
            world.EventBus.Register<ComponentModifiedEvent<Ecs0>>(this);
            world.EventBus.Register<ComponentRemovedEvent<Ecs0>>(this);

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

        public void OnEvent(ComponentCopiedEvent<Ecs0> e)
        {
            ComponentCopiedCount++;
        }

        public void OnEvent(ComponentAddedEvent<Ecs0> e)
        {
            ComponentAddedCount++;
        }

        public void OnEvent(ComponentModifiedEvent<Ecs0> e)
        {
            ComponentModifiedCount++;
        }

        public void OnEvent(ComponentRemovedEvent<Ecs0> e)
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
