using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Stores component type mapping information for an archetype and its chunks.
/// </summary>
internal struct ArchetypeComponentLookup
{
    /// <summary>
    /// Map from column index to the component ID.
    /// </summary>
    internal readonly ComponentId[] ComponentIdByColumnIndex;

    /// <summary>
    /// Sparse map from component ID to column index.
    /// </summary>
    internal readonly int[] ColumnIndexByComponentId;

    /// <summary>
    /// Sparse map from component ID to component dispatcher.
    /// </summary>
    internal readonly ComponentDispatcher[] ComponentDispatcherByComponentId;

    public ArchetypeComponentLookup(ImmutableOrderedListSet<ComponentId> components)
    {
        // Calculate max component ID
        var maxComponentId = int.MinValue;
        foreach (var component in components)
        {
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
            var component = components[i];
            ComponentIdByColumnIndex[i] = component;
            ColumnIndexByComponentId[component.Value] = i;
        }

        // Create a sparse map from component ID to component dispatcher
        ComponentDispatcherByComponentId = maxComponentId == int.MinValue ? [] : new ComponentDispatcher[maxComponentId + 1];
        foreach (var component in components)
        {
            ComponentDispatcherByComponentId[component.Value] = ComponentRegistry.GetComponentDispatcher(component);
        }
    }
}
