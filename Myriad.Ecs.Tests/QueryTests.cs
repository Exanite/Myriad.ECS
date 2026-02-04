using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class QueryTests
{
    [Fact]
    public void IncludeMatchNone()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<EcsFloat>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(0, archetypes.Length);
    }

    [Fact]
    public void IncludeMatchOne()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);

        var query = new QueryFilter()
            .Include<EcsFloat>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(1, archetypes.Length);
    }

    [Fact]
    public void IncludeMatchCaching()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);

        // Query that matches just one of the archetypes in the world
        var query = new QueryFilter()
            .Include<EcsFloat>()
            .Build(world);

        // Match once, check it matches one archetype
        var archetypes1 = query.Archetypes;
        Assert.Equal(1, archetypes1.Length);

        // Add an archetype to the world that the query should match
        var components1 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<EcsInt32>(), ComponentId.Get<EcsFloat>() });
        world.GetOrCreateArchetype(components1);

        // Check it now matches 2 archetypes
        var archetypes2 = query.Archetypes;
        Assert.Equal(2, archetypes2.Length);
        foreach (var archetype in archetypes2)
        {
            Assert.True(archetype.Components.Contains(ComponentId.Get<EcsFloat>()));
        }

        // Add an archetype to the world that the query should NOT match
        var components2 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<EcsInt32>(), ComponentId.Get<EcsByte>() });
        world.GetOrCreateArchetype(components2);

        // Check it now matches 2 archetypes
        var archetypes3 = query.Archetypes;
        Assert.Equal(2, archetypes3.Length);
        foreach (var archetype in archetypes3)
        {
            Assert.True(archetype.Components.Contains(ComponentId.Get<EcsFloat>()));
        }
    }

    [Fact]
    public void IncludeMatchMultiple()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>()]);

        var query = new QueryFilter()
            .Include<EcsFloat>()
            .Build(world);

        var archetypes = query.ArchetypesList;
        Assert.Equal(2, archetypes.Count);

        Assert.True(archetypes.All(x => x.Components.Contains(ComponentId.Get<EcsFloat>())));
    }

    [Fact]
    public void IncludeExclude()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>()]);

        var query = new QueryFilter()
            .Include<EcsFloat>()
            .Exclude<EcsInt32>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(1, archetypes.Length);

        var archetype = archetypes[0];
        Assert.True(archetype.Components.Contains(ComponentId.Get<EcsFloat>()));
        Assert.False(archetype.Components.Contains(ComponentId.Get<EcsInt32>()));
    }

    [Fact]
    public void ExactlyOne()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt16>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>()]);

        var query = new QueryFilter()
            .ExactlyOne<EcsFloat>()
            .ExactlyOne<EcsInt32>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(2, archetypes.Length);

        foreach (var archetype in archetypes)
        {
            Assert.True(archetype.Components.Count == 1);
        }
    }

    [Fact]
    public void AtLeastOne()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt16>(), ComponentId.Get<EcsInt64>()]);

        var query = new QueryFilter()
            .AtLeastOne<EcsFloat>()
            .AtLeastOne<EcsInt32>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(3, archetypes.Length);

        foreach (var archetype in archetypes)
        {
            Assert.True(archetype.Components.Contains(ComponentId.Get<EcsInt32>()) || archetype.Components.Contains(ComponentId.Get<EcsFloat>()));
        }
    }

    [Fact]
    public void NotAll()
    {
        var world = new EcsWorld();
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsInt16>(), ComponentId.Get<EcsInt64>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>()]);
        world.GetOrCreateArchetype([ComponentId.Get<EcsFloat>(), ComponentId.Get<EcsInt32>(), ComponentId.Get<EcsInt64>()]);

        var query = new QueryFilter()
            .NotAll<EcsFloat>()
            .NotAll<EcsInt32>()
            .Build(world);

        var archetypes = query.Archetypes;
        Assert.Equal(3, archetypes.Length);
    }

    [Fact]
    public void First_ThrowsNoMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        Assert.Throws<GuardException>(() => query.First());
    }

    [Fact]
    public void First_MatchSingle()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.Equal(entity, query.First());
    }

    [Fact]
    public void First_MatchMultiple()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity1 = commandBuffer.Create().Set(new Ecs0()).Entity;
        var entity2 = commandBuffer.Create().Set(new Ecs0()).Entity;

        commandBuffer.Execute();

        Assert.True(new[] { entity1, entity2 }.Contains(query.First()));
    }

    [Fact]
    public void FirstOrDefault_NullNoMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        Assert.False(query.FirstOrDefault().IsAlive);
    }

    [Fact]
    public void FirstOrDefault_MatchSingle()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.Equal(entity, query.FirstOrDefault());
    }

    [Fact]
    public void FirstOrDefault_MatchMultiple()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity1 = commandBuffer.Create().Set(new Ecs0()).Entity;
        var entity2 = commandBuffer.Create().Set(new Ecs0()).Entity;
        commandBuffer.Execute();

        Assert.True(new[] { entity1, entity2 }.Contains(query.FirstOrDefault()));
    }

    [Fact]
    public void SingleOrDefault_NullNoMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        Assert.False(query.SingleOrDefault().IsAlive);
    }

    [Fact]
    public void SingleOrDefault_ThrowsMultipleMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.Throws<GuardException>(() => query.SingleOrDefault());
    }

    [Fact]
    public void SingleOrDefault_MatchSingle()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0()).Entity;
        commandBuffer.Execute();

        Assert.Equal(entity, query.SingleOrDefault());
    }

    [Fact]
    public void Single_ThrowsMultipleMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.Throws<GuardException>(() => query.Single());
    }

    [Fact]
    public void Single_ThrowsNoMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        Assert.Throws<GuardException>(() => query.Single());
    }

    [Fact]
    public void Single_MatchSingle()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0()).Entity;
        commandBuffer.Execute();

        Assert.Equal(entity, query.Single());
    }

    [Fact]
    public void Any_True()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.True(query.Any());
    }

    [Fact]
    public void Any_False()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        Assert.False(query.Any());
    }

    [Fact]
    public void Contains_True()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        Assert.True(query.IsMatch(entity));
    }

    [Fact]
    public void Contains_False()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create();
        commandBuffer.Execute();

        Assert.False(query.IsMatch(entity));
    }

    [Fact]
    public void Random_NullNoMatch()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var random = new Random(123);
        Assert.False(query.RandomOrDefault(random).IsAlive);
    }

    [Fact]
    public void Random_MatchSingle()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<Ecs0>()
            .Build(world);

        var commandBuffer = world.AcquireCommandBuffer();
        var entity = commandBuffer.Create().Set(new Ecs0());
        commandBuffer.Execute();

        var random = new Random(123);
        Assert.Equal(entity, query.RandomOrDefault(random));
    }

    [Fact]
    public void Random_MatchRandom()
    {
        var world = new EcsWorld();
        var query = new QueryFilter()
            .Include<EcsInt32>()
            .Build(world);

        var entities = new HashSet<Entity>();
        var commandBuffer = world.AcquireCommandBuffer();
        for (var i = 0; i < 10000; i++)
        {
            entities.Add(commandBuffer.Create().Set(new EcsInt32(i)));
        }

        for (var i = 0; i < 10000; i++)
        {
            entities.Add(commandBuffer.Create().Set(new EcsInt32(i)).Set(new Ecs0()));
        }

        for (var i = 0; i < 10000; i++)
        {
            entities.Add(commandBuffer.Create().Set(new EcsInt32(i)).Set(new Ecs1()));
        }

        commandBuffer.Execute();

        var random = new Random(123);
        for (var i = 0; i < 1000; i++)
        {
            Assert.True(entities.Contains(query.RandomOrDefault(random)));
        }
    }
}
