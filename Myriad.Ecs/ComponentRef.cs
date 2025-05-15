using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public readonly record struct ComponentRef<T> where T : IComponent
{
    public readonly Entity Entity;

    /// <summary>
    /// Returns a mutable reference to the component data.
    /// <br/>
    /// Will throw an exception if the Entity is not alive or if the component does not exist.
    /// If <see cref="IsAlive"/> is <see langword="true"/>, then accessing this property is safe.
    /// </summary>
    public ref T Value => ref Entity.GetComponent<T>();

    /// <summary>
    /// Is the component alive? If <see langword="false"/>, accessing <see cref="Value"/> will throw an exception.
    /// </summary>
    public bool IsAlive => Entity.IsAlive && Entity.HasComponent<T>();

    internal ComponentRef(Entity entity)
    {
        GuardUtility.IsTrue(entity.IsAlive, "Entity does not exist");
        GuardUtility.IsTrue(entity.HasComponent<T>(), $"Component does not exist on entity: {entity.GetType().Name}");

        Entity = entity;
    }
}
