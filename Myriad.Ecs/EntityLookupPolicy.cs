namespace Exanite.Myriad.Ecs;

public enum EntityLookupPolicy
{
    /// <summary>
    /// Returns the original entity if the entity does not exist in the lookup.
    /// </summary>
    PreserveIfNotExist,

    /// <summary>
    /// Returns a default entity if the entity does not exist in the lookup.
    /// </summary>
    DefaultIfNotExist,

    /// <summary>
    /// Throws if the entity does not exist in the lookup.
    /// </summary>
    ThrowIfNotExist,
}
