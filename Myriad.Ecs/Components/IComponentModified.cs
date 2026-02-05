namespace Exanite.Myriad.Ecs.Components;

public interface IComponentModified
{
    /// <summary>
    /// Called when the component is modified on an entity.
    /// </summary>
    public void OnModified();
}
