namespace Exanite.Myriad.Ecs.Components;

public interface IComponentCopied
{
    /// <summary>
    /// Called when the component is copied to a new world.
    /// </summary>
    /// <remarks>
    /// This is copied for the copy of the component in the destination world, but not the source world.
    /// </remarks>
    public void OnCopied(EcsWorld newWorld, IEntityLookup lookup);
}
