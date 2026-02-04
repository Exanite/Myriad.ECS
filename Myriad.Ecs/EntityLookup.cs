using System.Collections.Generic;

namespace Exanite.Myriad.Ecs;

public class EntityLookup : IEntityLookup
{
    public readonly Dictionary<Entity, Entity> Entries;

    public EntityLookup()
    {
        Entries = [];
    }

    public EntityLookup(Dictionary<Entity, Entity> entries)
    {
        Entries = entries;
    }

    public void Add(Entity from, Entity to)
    {
        Entries.Add(from, to);
    }

    public EcsRef<T> Get<T>(EcsRef<T> from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent
    {
        return new EcsRef<T>(Get(from.Entity, policy));
    }

    public Entity Get(Entity from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist)
    {
        if (Entries.TryGetValue(from, out var to))
        {
            return to;
        }

        return EntityLookupUtility.HandleLookupPolicy(from, policy);
    }
}
