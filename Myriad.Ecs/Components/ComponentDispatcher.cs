using System.Reflection;
using System.Runtime.CompilerServices;
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

    internal abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, IEntityLookup lookup);
    internal abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup);

    internal abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length);
    internal abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity);

    internal abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length);
    internal abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity);

    internal abstract void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length);
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

    internal static void ComponentModified<T>(ref T component) where T : IComponent, IComponentModified
    {
        component.OnModified();
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

/// <inheritdoc/>
/// <remarks>
/// The type check if statements should be dead code eliminated when the JIT generic specializes each method.
/// </remarks>
internal class ComponentDispatcher<T> : ComponentDispatcher where T : IComponent
{
    private delegate void ComponentSelfReferenceAction(ref T component, Entity entity);

    private delegate void ComponentCopiedAction(ref T component, EcsWorld newWorld, IEntityLookup lookup);

    private delegate void ComponentAction(ref T component);

    private readonly ComponentSelfReferenceAction? componentSelfReference;
    private readonly ComponentCopiedAction? componentCopied;
    private readonly ComponentAction? componentAdded;
    private readonly ComponentAction? componentModified;
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

        if (typeof(T).IsAssignableTo(typeof(IComponentModified)))
        {
            componentModified = (ComponentAction)typeof(ComponentDispatcher).GetMethod(nameof(ComponentModified), BindingFlags.NonPublic | BindingFlags.Static)!
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

    internal override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, IEntityLookup lookup)
    {
        if (length == 0)
        {
            return;
        }

        var world = archetype.World;
        RaiseInterfaceSelfReference(archetype, startIndex, length);
        RaiseInterfaceCopied(archetype, startIndex, length, lookup, world);
        RaiseWorldCopied(recursiveCommandBuffer, archetype, startIndex, length, lookup, world);
    }

    internal override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup)
    {
        var world = entity.World;
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentCopied)
        {
            componentCopied!.Invoke(ref component, world, lookup);
        }

        world.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entity, ref component, lookup));
    }

    internal override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length)
    {
        if (length == 0)
        {
            return;
        }

        var world = archetype.World;
        RaiseInterfaceSelfReference(archetype, startIndex, length);
        RaiseInterfaceAdded(archetype, startIndex, length);
        RaiseWorldAdded(recursiveCommandBuffer, archetype, startIndex, length, world);
    }

    internal override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        var world = entity.World;
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentAdded)
        {
            componentAdded!.Invoke(ref component);
        }

        world.EventBus.Raise(new ComponentAddedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    internal override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length)
    {
        if (length == 0)
        {
            return;
        }

        var world = archetype.World;
        RaiseInterfaceModified(archetype, startIndex, length);
        RaiseWorldModified(recursiveCommandBuffer, archetype, startIndex, length, world);
    }

    internal override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        var world = entity.World;
        ref var component = ref entity.Get<T>();

        if (component is IComponentModified)
        {
            componentModified!.Invoke(ref component);
        }

        world.EventBus.Raise(new ComponentModifiedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    internal override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length)
    {
        if (length == 0)
        {
            return;
        }

        // Notify world first before component
        // Component does the final cleanup
        var world = archetype.World;
        RaiseWorldRemoved(recursiveCommandBuffer, archetype, startIndex, length, world);
        RaiseInterfaceRemoved(archetype, startIndex, length);
    }

    internal override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        var world = entity.World;
        ref var component = ref entity.Get<T>();

        // Notify world first before component
        // Component does the final cleanup
        world.EventBus.Raise(new ComponentRemovedEvent<T>(recursiveCommandBuffer, entity, ref component));

        if (component is IComponentRemoved)
        {
            componentRemoved!.Invoke(ref component);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseInterfaceSelfReference(Archetype archetype, int startIndex, int length)
    {
        if (componentSelfReference == null)
        {
            return;
        }

        var components = archetype.GetSpan<T>();
        var entities = archetype.Entities;
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            componentSelfReference!.Invoke(ref components[entityIndex], entities[entityIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseInterfaceCopied(Archetype archetype, int startIndex, int length, IEntityLookup lookup, EcsWorld world)
    {
        if (componentCopied == null)
        {
            return;
        }

        var components = archetype.GetSpan<T>();
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            componentCopied!.Invoke(ref components[entityIndex], world, lookup);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RaiseWorldCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, IEntityLookup lookup, EcsWorld world)
    {
        var components = archetype.GetSpan<T>();
        var entities = archetype.Entities;
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            world.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entities[entityIndex], ref components[entityIndex], lookup));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseInterfaceAdded(Archetype archetype, int startIndex, int length)
    {
        if (componentAdded == null)
        {
            return;
        }

        var components = archetype.GetSpan<T>();
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            componentAdded!.Invoke(ref components[entityIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RaiseWorldAdded(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, EcsWorld world)
    {
        var components = archetype.GetSpan<T>();
        var entities = archetype.Entities;
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            world.EventBus.Raise(new ComponentAddedEvent<T>(recursiveCommandBuffer, entities[entityIndex], ref components[entityIndex]));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseInterfaceModified(Archetype archetype, int startIndex, int length)
    {
        if (componentModified == null)
        {
            return;
        }

        var components = archetype.GetSpan<T>();
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            componentModified!.Invoke(ref components[entityIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RaiseWorldModified(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, EcsWorld world)
    {
        var components = archetype.GetSpan<T>();
        var entities = archetype.Entities;
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            world.EventBus.Raise(new ComponentModifiedEvent<T>(recursiveCommandBuffer, entities[entityIndex], ref components[entityIndex]));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseInterfaceRemoved(Archetype archetype, int startIndex, int length)
    {
        if (componentRemoved == null)
        {
            return;
        }

        var components = archetype.GetSpan<T>();
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            componentRemoved!.Invoke(ref components[entityIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RaiseWorldRemoved(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, int startIndex, int length, EcsWorld world)
    {
        var components = archetype.GetSpan<T>();
        var entities = archetype.Entities;
        for (var i = 0; i < length; i++)
        {
            var entityIndex = startIndex + i;
            world.EventBus.Raise(new ComponentRemovedEvent<T>(recursiveCommandBuffer, entities[entityIndex], ref components[entityIndex]));
        }
    }
}
