using System.Collections.Generic;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EntityCopyTests
{
    [Fact]
    public void CopyFrom_CopiesComponentData()
    {
        var floatValue = 1f;
        var byteValue = (byte)2;
        var int32Value = 3;

        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsFloat(floatValue))
                .Set(new EcsByte(byteValue))
                .Set(new EcsInt32(int32Value));

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        // Check component values
        Assert.Equal(floatValue, GetSingle<EcsFloat>(dstWorld).Value);
        Assert.Equal(byteValue, GetSingle<EcsByte>(dstWorld).Value);
        Assert.Equal(int32Value, GetSingle<EcsInt32>(dstWorld).Value);
    }

    [Fact]
    public void CopyFrom_UpdatesSelfReference()
    {
        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsSelfReference());

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        Assert.Equal(srcWorld, GetSingle<EcsSelfReference>(srcWorld).Self.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsSelfReference>(dstWorld).Self.Entity.World);
    }

    [Fact]
    public void CopyFrom_OnePrefab_To_ManyEntities()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var prefab = srcCommandBuffer.Create().Set(new EcsSelfReference()).Entity;
        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        var newEntity1 = dstCommandBuffer.Create().CopyFrom(prefab).Entity;
        var newEntity2 = dstCommandBuffer.Create().CopyFrom(prefab).Entity;
        var newEntity3 = dstCommandBuffer.Create().CopyFrom(prefab).Entity;
        dstCommandBuffer.Execute();

        Assert.Equal(srcWorld, GetSingle<EcsSelfReference>(srcWorld).Self.Entity.World);
        Assert.Equal(newEntity1, newEntity1.Get<EcsSelfReference>().Self.Entity);
        Assert.Equal(newEntity2, newEntity2.Get<EcsSelfReference>().Self.Entity);
        Assert.Equal(newEntity3, newEntity3.Get<EcsSelfReference>().Self.Entity);

        Assert.NotEqual(newEntity1, newEntity2);
        Assert.NotEqual(newEntity2, newEntity3);
        Assert.NotEqual(newEntity3, newEntity1);
    }

    [Fact]
    public void CopyFrom_ManyPrefab_To_OneEntity()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var prefab1 = srcCommandBuffer.Create().Set(new Ecs1()).Set(new EcsSelfReference()).Set(new EcsByte(1)).Entity;
        var prefab2 = srcCommandBuffer.Create().Set(new Ecs2()).Set(new EcsSelfReference()).Set(new EcsByte(2)).Entity;
        var prefab3 = srcCommandBuffer.Create().Set(new Ecs3()).Set(new EcsSelfReference()).Set(new EcsByte(3)).Entity;
        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        var newEntity = dstCommandBuffer.Create()
            .CopyFrom(prefab1)
            .CopyFrom(prefab2)
            .CopyFrom(prefab3)
            .Entity;

        dstCommandBuffer.Execute();

        // Check structure
        Assert.True(newEntity.Has<Ecs1>());
        Assert.True(newEntity.Has<Ecs2>());
        Assert.True(newEntity.Has<Ecs3>());
        Assert.True(newEntity.Has<EcsSelfReference>());
        Assert.True(newEntity.Has<EcsByte>());

        // Check references
        Assert.Equal(prefab1, prefab1.Get<EcsSelfReference>().Self.Entity);
        Assert.Equal(prefab2, prefab2.Get<EcsSelfReference>().Self.Entity);
        Assert.Equal(prefab3, prefab3.Get<EcsSelfReference>().Self.Entity);
        Assert.Equal(newEntity, newEntity.Get<EcsSelfReference>().Self.Entity);

        // Last set should take precedence
        Assert.Equal(3, newEntity.Get<EcsByte>().Value);
    }

    [Fact]
    public void CopyFrom_AndRemovePrefabComponent()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var prefab = srcCommandBuffer.Create()
            .Set(new Ecs0())
            .Set(new Ecs1())
            .Set(new Ecs2())
            .Set(new Ecs3())
            .Entity;

        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        var newEntity = dstCommandBuffer.Create()
            .CopyFrom(prefab)
            .Remove<Ecs3>()
            .Entity;

        dstCommandBuffer.Execute();

        // Check structure
        Assert.True(newEntity.Has<Ecs0>());
        Assert.True(newEntity.Has<Ecs1>());
        Assert.True(newEntity.Has<Ecs2>());
        Assert.False(newEntity.Has<Ecs3>());
    }

    [Fact]
    public void CopyFrom_AndOverwritePrefabComponent()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var prefab = srcCommandBuffer.Create()
            .Set(new EcsInt16(16))
            .Set(new EcsInt32(32))
            .Entity;

        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        var newEntity = dstCommandBuffer.Create()
            .CopyFrom(prefab)
            .Set(new EcsInt32(123))
            .Entity;

        dstCommandBuffer.Execute();

        // Check structure
        Assert.True(newEntity.Has<EcsInt16>());
        Assert.True(newEntity.Has<EcsInt32>());

        // Check values
        Assert.Equal(16, newEntity.Get<EcsInt16>().Value);
        Assert.Equal(123, newEntity.Get<EcsInt32>().Value);
    }

    [Fact]
    public void CopyFrom_UpdatesOtherReferences()
    {
        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsCopied());

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self.Entity.World);

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self2.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self2.Entity.World);

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self3.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self3.World);
    }

    [Fact]
    public void CopyFrom_CreateHierarchy()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var srcChild1 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcChild2 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcRoot = srcCommandBuffer.Create().Set(new EcsTransform()
        {
            Children = [srcChild1.GetEcsRefUnchecked<EcsTransform>(), srcChild2.GetEcsRefUnchecked<EcsTransform>()],
        });

        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        // Order shouldn't matter
        var dstChild2 = dstCommandBuffer.Create().CopyFrom(srcChild2);
        var dstRoot = dstCommandBuffer.Create().CopyFrom(srcRoot).Entity;
        var dstChild1 = dstCommandBuffer.Create().CopyFrom(srcChild1);

        dstCommandBuffer.Execute();

        foreach (var entity in new QueryFilter().Build(dstWorld).EnumerateEntities())
        {
            Assert.True(entity.Has<EcsTransform>());

            ref var transform = ref entity.Get<EcsTransform>();
            Assert.Equal(dstWorld, transform.Self.Entity.World);

            foreach (var child in transform.Children)
            {
                Assert.Equal(dstWorld, child.Entity.World);
            }
        }

        Assert.Equal(2, dstRoot.Get<EcsTransform>().Children.Count);
        Assert.Equal(dstChild1, dstRoot.Get<EcsTransform>().Children[0].Entity);
        Assert.Equal(dstChild2, dstRoot.Get<EcsTransform>().Children[1].Entity);
    }

    [Fact]
    public void CopyFrom_CreateManyHierarchies()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var srcChild1 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcChild2 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcRoot = srcCommandBuffer.Create().Set(new EcsTransform()
        {
            Children = [srcChild1.GetEcsRefUnchecked<EcsTransform>(), srcChild2.GetEcsRefUnchecked<EcsTransform>()],
        });

        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        for (var i = 0; i < 20; i++)
        {
            var dstRoot = dstCommandBuffer.Create().CopyFrom(srcRoot).Set(new EcsInt32(i));
            dstCommandBuffer.Create().CopyFrom(srcChild1, dstRoot).Set(new EcsInt32(i));
            dstCommandBuffer.Create().CopyFrom(srcChild2, dstRoot).Set(new EcsInt32(i));
        }

        dstCommandBuffer.Execute();

        foreach (var entity in new QueryFilter().Build(dstWorld).EnumerateEntities())
        {
            Assert.True(entity.Has<EcsTransform>());

            ref var transform = ref entity.Get<EcsTransform>();
            Assert.Equal(entity, transform.Self.Entity);

            if (transform.Children.Count == 2)
            {
                var groupIndex = entity.Get<EcsInt32>().Value;

                Assert.Equal(groupIndex, transform.Children[0].Entity.Get<EcsInt32>().Value);
                Assert.Equal(groupIndex, transform.Children[1].Entity.Get<EcsInt32>().Value);

                Assert.Equal(dstWorld, transform.Children[0].Entity.World);
                Assert.Equal(dstWorld, transform.Children[1].Entity.World);

                Assert.NotEqual(transform.Children[0], transform.Children[1]);
                Assert.NotEqual(transform.Children[0].Entity, transform.Children[1].Entity);
            }
        }
    }

    private T GetSingle<T>(EcsWorld world) where T : IComponent
    {
        return new QueryFilter().Include<T>().Build(world).First().Get<T>();
    }

    private struct EcsSelfReference : IComponent, IComponentSelfReference<EcsSelfReference>
    {
        public EcsRef<EcsSelfReference> Self { get; set; }
    }

    private struct EcsCopied : IComponent, IComponentSelfReference<EcsCopied>, IComponentAdded, IComponentCopied
    {
        public EcsRef<EcsCopied> Self { get; set; }

        // Self automatically gets updated
        // These allow for testing OnCopied's functionality separately
        public EcsRef<EcsCopied> Self2 { get; set; }
        public Entity Self3 { get; set; }

        public void OnAdded()
        {
            Self2 = Self;
            Self3 = Self.Entity;
        }

        public void OnCopied(EcsWorld newWorld, IEntityLookup lookup)
        {
            Self2 = lookup.Get(Self2);
            Self3 = lookup.Get(Self3);
        }
    }

    private struct EcsTransform : IComponent, IComponentSelfReference<EcsTransform>, IComponentCopied
    {
        public EcsRef<EcsTransform> Self { get; set; }

        public List<EcsRef<EcsTransform>> Children = new();

        public EcsTransform() {}

        public void OnCopied(EcsWorld newWorld, IEntityLookup lookup)
        {
            Children = [..Children];
            foreach (ref var child in Children.AsSpan())
            {
                child = lookup.Get(child);
            }
        }
    }
}
