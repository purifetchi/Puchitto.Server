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
    public IPuchittoSystemsProvider SystemsProvider { get; }

    /// <summary>
    /// The delegate for when a client joins this realm.
    /// </summary>
    public delegate Task ClientJoinedRealmEvent(Client client);
    
    /// <summary>
    /// The delegate for when a client leaves this realm.
    /// </summary>
    public delegate Task ClientLeftRealmEvent(Client client);
    
    /// <summary>
    /// Invoked when a client joins this realm.
    /// </summary>
    public event ClientJoinedRealmEvent? OnClientJoinedRealm;
    
    /// <summary>
    /// Invoked when a client leaves this realm.
    /// </summary>
    public event ClientLeftRealmEvent? OnClientLeftRealm;

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
        SystemsProvider = puchittoSystemsProvider;
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
            var ent = SystemsProvider.EntityFactory
                .CreateEntityFromLevelData(this, entityDefinition);
            
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

        if (OnClientJoinedRealm is not null)
        {
            await OnClientJoinedRealm.Invoke(client);
        }
    }
}