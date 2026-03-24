using Puchitto.Server.Management;
using Puchitto.Server.Realms;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Game.Entities;

public class EntityFactory
{
    private class FactoryDefinition<TEntity> : IFactoryDefinition
        where TEntity : BaseEntity, new()
    {
        public BaseEntity Build()
        {
            return new TEntity();
        }
    }
    
    private interface IFactoryDefinition
    {
        BaseEntity Build();
    }
    
    private readonly IPuchittoSystemsProvider _systemsProvider;

    private readonly Dictionary<string, IFactoryDefinition> _factories = new();

    public EntityFactory(IPuchittoSystemsProvider systemsProvider)
    {
        _systemsProvider = systemsProvider;
    }
    
    public void RegisterEntity<TEntity>(string name)
        where TEntity : BaseEntity, new()
    {
        var factoryDefinition = new FactoryDefinition<TEntity>();
        _factories[name] = factoryDefinition;
    }

    public TEntity CreateEntity<TEntity>(Realm realm)
        where TEntity : BaseEntity, new()
    {
        var entity = new TEntity
        {
            Id = realm.IdAllocator.GetNextId()
        };
        
        entity.Initialize(realm);

        return entity;
    }
    
    public BaseEntity CreateEntityFromLevelData(
        Realm realm,
        LevelEntityData levelEntityData)
    {
        BaseEntity entity;
        
        // If we do not know the type of this entity, it will always be an UnknownEntity.
        entity = !_factories.TryGetValue(levelEntityData.Type, out var factoryDefinition)
            ? new UnknownEntity()
            : factoryDefinition.Build();

        entity.Initialize(realm, levelEntityData);
        return entity;
    }
}