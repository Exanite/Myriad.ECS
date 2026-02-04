namespace Exanite.Myriad.Ecs.Components;

public interface IComponentRemoved
{
    /// <summary>
    /// Called when the component is removed from an entity.
    /// </summary>
    public void OnRemoved();
}
