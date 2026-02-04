using System.Reflection;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Provides a performant way to invoke generic methods without having access to the component types at compile time.
/// </summary>
public abstract class ComponentDispatcher
{
    public abstract TReturn Create<TFactory, TReturn>(TFactory factory) where TFactory : IComponentFactory<TReturn>;
    public abstract TReturn Create<TFactory, TInput, TReturn>(TFactory factory, TInput input) where TFactory : IComponentFactory<TInput, TReturn>;

    public abstract void Invoke<TAction>(TAction action) where TAction : IComponentAction;
    public abstract void Invoke<TAction, TInput>(TAction action, TInput input) where TAction : IComponentAction<TInput>;

    internal abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, IEntityLookup lookup);
    internal abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup);
    internal abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity);
    internal abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity);
    internal abstract void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Entity entity);

    internal static void ComponentSelfReference<T>(ref T component, Entity entity) where T : IComponent, IComponentSelfReference<T>
    {
        component.Self = entity.GetEcsRef<T>();
    }

    internal static void ComponentCopied<T>(ref T component, EcsWorld newWorld, IEntityLookup lookup) where T : IComponent, IComponentCopied
    {
        component.OnCopied(newWorld, lookup);
    }

    internal static void ComponentAdded<T>(ref T component) where T : IComponent, IComponentAdded
    {
        component.OnAdded();
    }

    internal static void ComponentSet<T>(ref T component) where T : IComponent, IComponentSet
    {
        component.OnSet();
    }

    internal static void ComponentRemoved<T>(ref T component) where T : IComponent, IComponentRemoved
    {
        component.OnRemoved();
    }

    public interface IComponentFactory<out TReturn>
    {
        public TReturn Create<T>() where T : IComponent;
    }

    public interface IComponentFactory<in TInput, out TReturn>
    {
        public TReturn Create<T>(TInput input) where T : IComponent;
    }

    public interface IComponentAction
    {
        public void Invoke<T>() where T : IComponent;
    }

    public interface IComponentAction<in TInput>
    {
        public void Invoke<T>(TInput input) where T : IComponent;
    }
}

internal class ComponentDispatcher<T> : ComponentDispatcher where T : IComponent
{
    private delegate void ComponentSelfReferenceAction(ref T component, Entity entity);

    private delegate void ComponentCopiedAction(ref T component, EcsWorld newWorld, IEntityLookup lookup);

    private delegate void ComponentAction(ref T component);

    private readonly ComponentSelfReferenceAction? componentSelfReference;
    private readonly ComponentCopiedAction? componentCopied;
    private readonly ComponentAction? componentAdded;
    private readonly ComponentAction? componentSet;
    private readonly ComponentAction? componentRemoved;

    public ComponentDispatcher()
    {
        if (typeof(T).IsAssignableTo(typeof(IComponentSelfReference<T>)))
        {
            componentSelfReference = (ComponentSelfReferenceAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentSelfReference), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentSelfReferenceAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentCopied)))
        {
            componentCopied = (ComponentCopiedAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentCopied), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentCopiedAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentAdded)))
        {
            componentAdded = (ComponentAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentAdded), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSet)))
        {
            componentSet = (ComponentAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentSet), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentRemoved)))
        {
            componentRemoved = (ComponentAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentRemoved), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }
    }

    public override TReturn Create<TFactory, TReturn>(TFactory factory) => factory.Create<T>();
    public override TReturn Create<TFactory, TInput, TReturn>(TFactory factory, TInput input) => factory.Create<T>(input);

    public override void Invoke<TAction>(TAction action) => action.Invoke<T>();
    public override void Invoke<TAction, TInput>(TAction action, TInput input) => action.Invoke<T>(input);

    internal override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, IEntityLookup lookup)
    {
        if (archetype.EntityCount == 0)
        {
            return;
        }

        if (componentSelfReference != null)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var components = chunk.GetSpan<T>();
                var entities = chunk.Entities;

                for (var i = 0; i < entities.Length; i++)
                {
                    componentSelfReference!.Invoke(ref components[i], entities[i]);
                }
            }
        }

        if (componentCopied != null)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    ref var component = ref entity.Get<T>();

                    componentCopied!.Invoke(ref component, entity.World, lookup);
                }
            }
        }

        foreach (var chunk in archetype.Chunks)
        {
            foreach (var entity in chunk.Entities)
            {
                ref var component = ref entity.Get<T>();

                entity.World.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entity, ref component, lookup));
            }
        }
    }

    internal override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentCopied)
        {
            componentCopied!.Invoke(ref component, entity.World, lookup);
        }

        entity.World.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entity, ref component, lookup));
    }

    internal override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentAdded)
        {
            componentAdded!.Invoke(ref component);
        }

        entity.World.EventBus.Raise(new ComponentAddedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    internal override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSet)
        {
            componentSet!.Invoke(ref component);
        }

        entity.World.EventBus.Raise(new ComponentModifiedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    internal override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();
        entity.World.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref component));

        if (component is IComponentRemoved)
        {
            componentRemoved!.Invoke(ref component);
        }
    }
}
