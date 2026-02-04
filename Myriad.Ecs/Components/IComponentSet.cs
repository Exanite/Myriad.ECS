namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSet
{
    /// <summary>
    /// Called when the component is set on an entity.
    /// </summary>
    public void OnSet();
}
