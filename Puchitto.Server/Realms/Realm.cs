using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

/// <summary>
/// A single realm within a Puchitto server.
/// </summary>
public class Realm
{
    /// <summary>
    /// Is this realm the default realm for loading?
    /// </summary>
    public bool IsDefault { get; private set; }
    
    /// <summary>
    /// The entity manager for this realm.
    /// </summary>
    public EntityManager EntityManager { get; }
    
    /// <summary>
    /// The ID allocator for this realm.
    /// </summary>
    public EntityIdAllocator IdAllocator { get; }

    /// <summary>
    /// The puchitto systems provider.
    /// </summary>
    private IPuchittoSystemsProvider _systemsProvider;

    /// <summary>
    /// Constructs a new Realm.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The systems provider.
    /// </param>
    public Realm(
        IPuchittoSystemsProvider puchittoSystemsProvider,
        Level levelDefinition,
        bool isDefault = false)
    {
        _systemsProvider = puchittoSystemsProvider;
        IsDefault = isDefault;
        EntityManager = new EntityManager(
            puchittoSystemsProvider.ClientManager,
            puchittoSystemsProvider.MakeLogger<EntityManager>());

        ParseEntities(levelDefinition);
        
        var maxId = EntityManager.Entities.Max(e => e.Id);
        IdAllocator = new EntityIdAllocator(maxId + 1);
    }

    /// <summary>
    /// Parses the entities within this realm.
    /// </summary>
    private void ParseEntities(Level levelDefinition)
    {
        foreach (var entityDefinition in levelDefinition.Entities)
        {
            var ent = _systemsProvider.EntityFactory
                .CreateEntityFromLevelData(entityDefinition);
            
            EntityManager.AddEntity(ent);
        }
    }
    
    /// <summary>
    /// Spawns a player given the rules.
    /// </summary>
    /// <param name="client">The connecting client.</param>
    /// <param name="rules">The rules.</param>
    public async Task SpawnPlayer(Client client, IGameServerRules rules)
    {
        client.CurrentRealm = this;

        await EntityManager.SpawnMissingEntitiesFor(client);

        var entity = rules.CreateEntityForClient(this);
        entity.Owner = client;
        
        await EntityManager.AddAndSpawnForEveryone(entity);   
    }
}