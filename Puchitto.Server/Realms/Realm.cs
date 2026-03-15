using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;

namespace Puchitto.Server.Realms;

/// <summary>
/// A single realm within a Puchitto server.
/// </summary>
public class Realm
{
    /// <summary>
    /// The entity manager for this realm.
    /// </summary>
    public EntityManager EntityManager { get; }
    
    /// <summary>
    /// The ID allocator for this realm.
    /// </summary>
    public EntityIdAllocator IdAllocator { get; }

    /// <summary>
    /// Constructs a new Realm.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The systems provider.
    /// </param>
    public Realm(
        IPuchittoSystemsProvider puchittoSystemsProvider)
    {
        EntityManager = new EntityManager(
            puchittoSystemsProvider.ClientManager,
            puchittoSystemsProvider.MakeLogger<EntityManager>());

        IdAllocator = new EntityIdAllocator(100); // TODO: This should be filled out with the last authored ID from the realm. For testing purposes let's start from 100.
    }

    /// <summary>
    /// Spawns a player given the rules.
    /// </summary>
    /// <param name="client">The connecting client.</param>
    /// <param name="rules">The rules.</param>
    public async Task SpawnPlayer(Client client, IGameServerRules rules)
    {
        await EntityManager.SpawnMissingEntitiesFor(client);
            
        var entity = rules.CreateEntityForClient();
        entity.Owner = client;
        await EntityManager.AddAndSpawnForEveryone(entity);   
    }
}