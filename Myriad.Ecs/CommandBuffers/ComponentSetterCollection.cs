using System.Collections.Generic;
using System.Diagnostics;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Collection of components of all different types, keyed by ComponentId, involves no boxing.
/// </summary>
internal class ComponentSetterCollection
{
    private readonly Dictionary<ComponentId, IComponentList> components = [];

    public void Clear()
    {
        foreach (var (_, value) in components)
        {
            value.Recycle();
        }

        components.Clear();
    }

    public SetterId Add<T>(T value) where T : IComponent
    {
        var id = ComponentId.Get<T>();

        if (!components.TryGetValue(id, out var list))
        {
            list = SimplePool<GenericComponentList<T>>.Acquire();
            components.Add(id, list);
        }

        var index = ((GenericComponentList<T>)list).Add(value);
        return new SetterId(id, index);
    }

    public void Overwrite<T>(SetterId index, T value) where T : IComponent
    {
        var id = ComponentId.Get<T>();
        ((GenericComponentList<T>)components[id]).Overwrite(index, value);
    }

    public void Write(SetterId id, EntityStorageLocation location)
    {
        var list = components[id.ComponentId];
        list.Write(id.Index, location);
    }

    public readonly struct SetterId
    {
        /// <summary>
        /// Component ID of the component being overwritten
        /// </summary>
        internal readonly ComponentId ComponentId;

        /// <summary>
        /// Index of the setter in the setters list
        /// </summary>
        internal readonly int Index;

        internal SetterId(ComponentId componentId, int index)
        {
            ComponentId = componentId;
            Index = index;
        }
    }

    #region Component list

    private interface IComponentList
    {
        public void Clear();

        void Recycle();

        void Write(int index, EntityStorageLocation location);
    }

    [DebuggerDisplay("Count = {values.Count}")]
    private class GenericComponentList<T> : IComponentList where T : IComponent
    {
        private readonly List<T> values = [];

        public void Clear()
        {
            values.Clear();
        }

        public int Add(T value)
        {
            values.Add(value);
            return values.Count - 1;
        }

        public void Overwrite(SetterId index, T value)
        {
            values[index.Index] = value;
        }

        public void Recycle()
        {
            Clear();
            SimplePool.Release(this);
        }

        public void Write(int index, EntityStorageLocation location)
        {
            location.GetMutable<T>() = values[index];
        }
    }

    #endregion
}
