using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Stores information about the components stored in an archetype.
/// </summary>
internal struct ArchetypeInfo
{
    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public readonly ImmutableOrderedListSet<ComponentId> Components;

    /// <summary>
    /// A bloom filter of all the components in this archetype.
    /// </summary>
    public readonly ComponentBloomFilter BloomFilter;

    /// <summary>
    /// The hash of all components IDs in this archetype.
    /// </summary>
    public readonly ArchetypeHash Hash;

    /// <summary>
    /// Map from column index to the component ID.
    /// </summary>
    public readonly ComponentId[] ComponentIdByColumnIndex;

    /// <summary>
    /// Sparse map from component ID to column index.
    /// </summary>
    public readonly int[] ColumnIndexByComponentId;

    /// <summary>
    /// Sparse map from component ID to component dispatcher.
    /// </summary>
    public readonly ComponentDispatcher[] ComponentDispatcherByComponentId;

    public ArchetypeInfo(ImmutableOrderedListSet<ComponentId> components)
    {
        Components = components;

        // Create bloom filter
        BloomFilter = components.ToBloomFilter();

        // Calculate max component ID and archetype hash
        var maxComponentId = int.MinValue;
        foreach (var component in components)
        {
            Hash = Hash.Toggle(component);
            if (component.Value > maxComponentId)
            {
                maxComponentId = component.Value;
            }
        }

        // Initialize a map from column index to component ID
        ComponentIdByColumnIndex = new ComponentId[components.Count];

        // Initialize a sparse map from component ID to column index
        ColumnIndexByComponentId = maxComponentId == int.MinValue ? [] : new int[maxComponentId + 1];
        Array.Fill(ColumnIndexByComponentId, -1);

        // Fill previously mentioned maps
        for (var i = 0; i < components.Count; i++)
        {
            var componentId = components[i];
            ComponentIdByColumnIndex[i] = componentId;
            ColumnIndexByComponentId[componentId.Value] = i;
        }

        // Create a sparse map from component ID to component dispatcher
        ComponentDispatcherByComponentId = maxComponentId == int.MinValue ? [] : new ComponentDispatcher[maxComponentId + 1];
        foreach (var component in components)
        {
            ComponentDispatcherByComponentId[component.Value] = ComponentRegistry.GetComponentDispatcher(component);
        }
    }
}
