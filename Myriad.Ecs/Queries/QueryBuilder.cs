using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Build a new <see cref="QueryDescription"/> object
/// </summary>
public sealed class QueryBuilder
{
    /// <summary>
    /// An Entity must include all of these components to be matched by this query
    /// </summary>
    public IReadOnlyList<ComponentId> Included => include.Items;
    private readonly ComponentSet include;

    /// <summary>
    /// Entities with these components will not be matched by this query
    /// </summary>
    public IReadOnlyList<ComponentId> Excluded => exclude.Items;
    private readonly ComponentSet exclude;

    /// <summary>
    /// At least one of all these components must be on an Entity for it to be matched by this query
    /// </summary>
    public IReadOnlyList<ComponentId> AtLeastOnes => atLeastOne.Items;
    private readonly ComponentSet atLeastOne;

    /// <summary>
    /// Exactly one of all these components must be on an Entity for it to be matched by this query
    /// </summary>
    public IReadOnlyList<ComponentId> ExactlyOnes => exactlyOne.Items;
    private readonly ComponentSet exactlyOne;

    /// <summary>
    /// Create a new <see cref="QueryBuilder"/>
    /// </summary>
    public QueryBuilder()
    {
        include = new ComponentSet(ContainsComponent, 0);
        exclude = new ComponentSet(ContainsComponent, 1);
        atLeastOne = new ComponentSet(ContainsComponent, 2);
        exactlyOne = new ComponentSet(ContainsComponent, 3);
    }

    /// <summary>
    /// Build a <see cref="QueryDescription"/> from the current state of this builder.
    /// </summary>
    public QueryDescription Build(EcsWorld world)
    {
        return new QueryDescription(
            world,
            include.ToImmutableSet(),
            exclude.ToImmutableSet(),
            atLeastOne.ToImmutableSet(),
            exactlyOne.ToImmutableSet()
        );
    }

    private void ContainsComponent(ComponentId id, int index, string caller)
    {
        if (index != include.Index && include.Contains(id))
        {
            throw new InvalidOperationException($"Cannot '{caller}' - component is already included");
        }

        if (index != exclude.Index && exclude.Contains(id))
        {
            throw new InvalidOperationException($"Cannot '{caller}' - component is already excluded");
        }

        if (index != atLeastOne.Index && atLeastOne.Contains(id))
        {
            throw new InvalidOperationException($"Cannot '{caller}' - component is already included (at least one)");
        }

        if (index != exactlyOne.Index && exactlyOne.Contains(id))
        {
            throw new InvalidOperationException($"Cannot '{caller}' - component is already included (exactly one)");
        }
    }

    #region Include

    /// <summary>
    /// The given component must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Include<T>() where T : IComponent
    {
        include.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Include(Type type)
    {
        include.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Include(ComponentId id)
    {
        include.Add(id);
        return this;
    }

    /// <summary>
    /// Check if the given component type has been marked as "Include"
    /// </summary>
    /// <param name="type">The component type.</param>
    /// <returns>true, if the component is included, otherwise false</returns>
    public bool IsIncluded(Type type)
    {
        return include.Contains(type);
    }

    /// <summary>
    /// Check if the given component type has been marked as "Include"
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>true, if the component is included, otherwise false</returns>
    public bool IsIncluded<T>() where T : IComponent
    {
        return include.Contains<T>();
    }

    /// <summary>
    /// Check if the given component type has been marked as "Include"
    /// </summary>
    /// <param name="id">The component id</param>
    /// <returns>true, if the component is included, otherwise false</returns>
    public bool IsIncluded(ComponentId id)
    {
        return include.Contains(id);
    }

    #endregion

    #region Exclude

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Exclude<T>() where T : IComponent
    {
        exclude.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Exclude(Type type)
    {
        exclude.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder Exclude(ComponentId id)
    {
        exclude.Add(id);
        return this;
    }

    /// <summary>
    /// Check if the given component is excluded
    /// </summary>
    public bool IsExcluded(Type type)
    {
        return exclude.Contains(type);
    }

    /// <summary>
    /// Check if the given component is excluded
    /// </summary>
    public bool IsExcluded<T>() where T : IComponent
    {
        return exclude.Contains<T>();
    }

    /// <summary>
    /// Check if the given component is excluded
    /// </summary>
    public bool IsExcluded(ComponentId id)
    {
        return exclude.Contains(id);
    }

    #endregion

    #region At least one

    /// <summary>
    /// At least one of all components specified as AtLeastOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder AtLeastOneOf<T>() where T : IComponent
    {
        atLeastOne.Add<T>();
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder AtLeastOneOf(Type type)
    {
        atLeastOne.Add(type);
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder AtLeastOneOf(ComponentId id)
    {
        atLeastOne.Add(id);
        return this;
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have at least one of
    /// </summary>
    public bool IsAtLeastOneOf(Type type)
    {
        return atLeastOne.Contains(type);
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have at least one of
    /// </summary>
    public bool IsAtLeastOneOf<T>() where T : IComponent
    {
        return atLeastOne.Contains<T>();
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have at least one of
    /// </summary>
    public bool IsAtLeastOneOf(ComponentId id)
    {
        return atLeastOne.Contains(id);
    }

    #endregion

    #region Exactly one

    /// <summary>
    /// Exactly one of all components specified as ExactlyOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder ExactlyOneOf<T>() where T : IComponent
    {
        exactlyOne.Add<T>();
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder ExactlyOneOf(Type type)
    {
        exactlyOne.Add(type);
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOneOf must exist for an entity to be matched by this query
    /// </summary>
    public QueryBuilder ExactlyOneOf(ComponentId id)
    {
        exactlyOne.Add(id);
        return this;
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have exactly one of
    /// </summary>
    public bool IsExactlyOneOf(Type type)
    {
        return exactlyOne.Contains(type);
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have exactly one of
    /// </summary>
    public bool IsExactlyOneOf<T>() where T : IComponent
    {
        return exactlyOne.Contains<T>();
    }

    /// <summary>
    /// Check if the given component is one of the components which entities must have exactly one of
    /// </summary>
    public bool IsExactlyOneOf(ComponentId id)
    {
        return exactlyOne.Contains(id);
    }

    #endregion

    private class ComponentSet(Action<ComponentId, int, string> check, int index)
    {
        public int Index { get; } = index;

        public readonly OrderedListSet<ComponentId> Items = [];

        private ImmutableOrderedListSet<ComponentId>? immutableSet;

        public ImmutableOrderedListSet<ComponentId> ToImmutableSet()
        {
            if (immutableSet != null)
            {
                return immutableSet;
            }

            immutableSet = Items.Count == 0 ? ImmutableOrderedListSet<ComponentId>.Empty : ImmutableOrderedListSet<ComponentId>.Create(Items);
            return immutableSet;
        }

        public bool Add(ComponentId id, [CallerMemberName] string caller = "")
        {
            check(id, Index, caller);
            if (Items.Add(id))
            {
                immutableSet = null;
                return true;
            }

            return false;
        }

        public bool Add(Type type, [CallerMemberName] string caller = "")
        {
            return Add(ComponentId.Get(type), caller);
        }

        public bool Add<T>([CallerMemberName] string caller = "") where T : IComponent
        {
            return Add(ComponentId.Get<T>(), caller);
        }

        public bool Contains(ComponentId id)
        {
            return Items.Contains(id);
        }

        public bool Contains(Type type)
        {
            return Contains(ComponentId.Get(type));
        }

        public bool Contains<T>() where T : IComponent
        {
            return Contains(ComponentId.Get<T>());
        }
    }
}
