namespace Exanite.Myriad.Ecs.Worlds;

internal struct EntityLocation
{
    /// <summary>
    /// The current version of this entity.
    /// </summary>
    public uint Version;

    /// <summary>
    /// The archetype that contains this entity.
    /// </summary>
    public Archetype Archetype;

    /// <summary>
    /// The location of the entity within the archetype it is stored in.
    /// </summary>
    public int IndexInArchetype;
}
