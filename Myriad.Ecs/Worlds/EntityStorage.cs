using System;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Contains the arrays used to store entity and component data.
/// </summary>
internal readonly struct EntityStorage
{
    /// <remarks>
    /// Indexed using entity index.
    /// </remarks>
    public readonly Entity[] EntityColumn;

    /// <remarks>
    /// Indexed using column index, then entity index.
    /// </remarks>
    public readonly Array[] ComponentColumns;

    /// <summary>
    /// The max number of entities this storage can hold.
    /// </summary>
    public readonly int Capacity;

    public EntityStorage(ref readonly ArchetypeInfo info, int capacity)
    {
        Capacity = capacity;

        EntityColumn = new Entity[capacity];
        ComponentColumns = new Array[info.ComponentIdByColumnIndex.Length];

        var arrayFactory = new ArrayFactory();
        for (var i = 0; i < ComponentColumns.Length; i++)
        {
            var dispatcher = info.ComponentDispatcherByComponentId[info.ComponentIdByColumnIndex[i].Value];
            ComponentColumns[i] = dispatcher.Create<ArrayFactory, int, Array>(arrayFactory, capacity);
        }
    }

    private readonly struct ArrayFactory : ComponentDispatcher.IComponentFactory<int, Array>
    {
        public Array Create<T>(int capacity) where T : IComponent
        {
            return capacity == 0 ? [] : new T[capacity];
        }
    }
}
