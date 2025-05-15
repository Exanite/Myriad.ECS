using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract void RaiseComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void RaiseComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void RaiseComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    public override void RaiseComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentAdded<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentModified<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }
}
