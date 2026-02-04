using System;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public static class EntityLookupUtility
{
    /// <summary>
    /// The default lookup policy handler for <see cref="IEntityLookup"/> implementations.
    /// Call this if there is no mapping for the specified entity.
    /// </summary>
    /// <param name="entity">The original entity used for the lookup.</param>
    /// <param name="policy">The policy requested by the caller.</param>
    public static Entity HandleLookupPolicy(Entity entity, EntityLookupPolicy policy)
    {
        return policy switch
        {
            EntityLookupPolicy.PreserveIfNotExist => entity,
            EntityLookupPolicy.DefaultIfNotExist => default,
            EntityLookupPolicy.ThrowIfNotExist => throw new InvalidOperationException("Entity does not exist in this lookup table"),
            _ => throw ExceptionUtility.NotSupportedEnumValue(policy),
        };
    }
}
