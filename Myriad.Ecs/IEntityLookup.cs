namespace Exanite.Myriad.Ecs;

public interface IEntityLookup
{
    public EcsRef<T> Get<T>(EcsRef<T> from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent;

    public Entity Get(Entity from, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist);
}
