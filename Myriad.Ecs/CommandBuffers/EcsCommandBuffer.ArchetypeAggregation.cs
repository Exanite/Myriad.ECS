using System.Collections.Generic;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public sealed partial class EcsCommandBuffer
{
    // Keep track of a fixed number of aggregation nodes. The root node (0) is the node for a new entity
    // with no components. Nodes store a list of "edges" leading to other nodes. Edges indicate
    // the addition of that component to the entity. Buffered entities keep track of their node ID. Every
    // buffered entity with the same node ID therefore has the same archetype. Except for node=-1, which
    // indicates unknown.

    private const int MaxAggregationEdges = 1024;

    /// <summary>
    /// Map from (currentArchetypeKey, addedComponent) => newArchetypeKey.
    /// </summary>
    private readonly Dictionary<(int, ComponentId), int> archetypeEdges = new();

    /// <summary>
    /// Given an archetype key and an added component, determine the new archetype key.
    /// </summary>
    private int GetArchetypeKey(int currentKey, ComponentId addedComponent)
    {
        if (!archetypeEdges.TryGetValue((currentKey, addedComponent), out var newKey))
        {
            // Limit the number of edges to prevent explosive growth in some edge cases.
            if (archetypeEdges.Count >= MaxAggregationEdges)
            {
                return -1;
            }

            newKey = archetypeEdges.Count + 1;
            archetypeEdges.Add((currentKey, addedComponent), newKey);
        }

        return newKey;
    }
}
