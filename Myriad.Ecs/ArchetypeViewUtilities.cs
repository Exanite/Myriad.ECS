using System;
using System.Collections;
using System.Collections.Generic;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

public static class ArchetypeViewUtilities
{
    /// <summary>
    /// Count how many entities match this query.
    /// </summary>
    public static int Count(this IArchetypeView view)
    {
        var count = 0;
        foreach (var archetype in view.Archetypes)
        {
            count += archetype.EntityCount;
        }

        return count;
    }

    /// <summary>
    /// Check if this query matches any entities.
    /// </summary>
    public static bool Any(this IArchetypeView view)
    {
        foreach (var archetype in view.Archetypes)
        {
            if (archetype.EntityCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get the first entity this query matches. Returns default if none.
    /// </summary>
    public static Entity FirstOrDefault(this IArchetypeView view)
    {
        foreach (var archetype in view.Archetypes)
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.Entities.Length > 0)
                {
                    return chunk.Entities[0];
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Get the first entity this query matches. Throws if none.
    /// </summary>
    public static Entity First(this IArchetypeView view)
    {
        var entity = view.FirstOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.First() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get the single entity this query matches. Returns default if none.
    /// </summary>
    public static Entity SingleOrDefault(this IArchetypeView view)
    {
        Entity entity = default;
        foreach (var archetype in view.Archetypes)
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.Entities.Length == 0)
                {
                    continue;
                }

                GuardUtility.IsFalse(chunk.Entities.Length > 1 || entity.IsAlive, "QueryView.SingleOrDefault() found more than one matching entity");

                entity = chunk.Entities[0];
            }
        }

        return entity;
    }

    /// <summary>
    /// Get the single entity this query matches. Throws if none.
    /// </summary>
    public static Entity Single(this IArchetypeView view)
    {
        var entity = view.SingleOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.SingleOrDefault() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get a random entity this query matches. Returns default if none.
    /// </summary>
    public static Entity RandomOrDefault(this IArchetypeView view, Random random)
    {
        // Get total entity count
        var count = view.Count();
        if (count == 0)
        {
            return default;
        }

        // Choose the index of the entity
        var choice = random.Next(0, count);

        // Find that entity
        foreach (var archetype in view.Archetypes)
        {
            // Check if it's within this archetype, if not move to the next archetype
            if (choice - archetype.EntityCount >= 0)
            {
                choice -= archetype.EntityCount;
            }
            else
            {
                // Step through chunks
                foreach (var chunk in archetype.Chunks)
                {
                    // Check if it's within this chunk, if not move to next chunk
                    if (choice - chunk.Entities.Length >= 0)
                    {
                        choice -= chunk.Entities.Length;
                    }
                    else
                    {
                        return chunk.Entities[choice];
                    }
                }
            }
        }

        // This shouldn't happen
        return default;
    }

    /// <summary>
    /// Get a random entity this query matches. Throws if none.
    /// </summary>
    public static Entity Random(this IArchetypeView view, Random random)
    {
        var entity = view.RandomOrDefault(random);
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.RandomOrDefault() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Iterate through every entity.
    /// </summary>
    /// <remarks>
    /// This is slow! Use only for debugging.
    /// </remarks>
    public static ViewEntityEnumerable EnumerateEntities(this IArchetypeView view)
    {
        return new ViewEntityEnumerable(view);
    }

    public readonly struct ViewEntityEnumerable : IEnumerable<Entity>
    {
        private readonly IArchetypeView view;

        public ViewEntityEnumerable(IArchetypeView view)
        {
            this.view = view;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(view);
        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<Entity>
        {
            private readonly IArchetypeView view;

            private Archetype? archetype;
            private int archetypeIndex = 0;

            private Chunk? chunk;
            private int chunkIndex = 0;

            public Entity Current { get; private set; }
            private int entityIndex = -1;

            object IEnumerator.Current => Current;

            public Enumerator(IArchetypeView view)
            {
                this.view = view;

                if (view.Archetypes.Length > 0)
                {
                    archetype = view.Archetypes[0];
                    if (archetype.Chunks.Length > 0)
                    {
                        chunk = archetype.Chunks[0];
                    }
                }
            }

            public bool MoveNext()
            {
                if (archetype == null || chunk == null)
                {
                    // This means the constructor found no matches
                    return false;
                }

                entityIndex++;
                while (entityIndex >= chunk.Entities.Length)
                {
                    // Move to next chunk if needed
                    entityIndex = 0;
                    chunkIndex++;
                    while (chunkIndex >= archetype.Chunks.Length)
                    {
                        // Move to next archetype if needed
                        chunkIndex = 0;
                        archetypeIndex++;

                        if (archetypeIndex >= view.Archetypes.Length)
                        {
                            // Enumeration complete
                            return false;
                        }

                        archetype = view.Archetypes[archetypeIndex];
                    }

                    chunk = archetype.Chunks[chunkIndex];
                }

                Current = chunk.Entities[entityIndex];
                return true;
            }

            public void Reset() => throw new NotSupportedException();
            public void Dispose() {}
        }
    }
}
