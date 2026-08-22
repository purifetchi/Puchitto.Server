using Puchitto.Server.Clients;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets;
using Puchitto.Server.Realms;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Game;

/// <summary>
/// Custom rules defined by the current game.
/// </summary>
public interface IGameServerRules
{
    /// <summary>
    /// The name of the game rules being run.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// The systems provider.
    /// </summary>
    IPuchittoSystemsProvider PuchittoSystemsProvider { get; set; }
    
    /// <summary>
    /// The realm registry for this game.
    /// </summary>
    IRealmRegistry RealmRegistry { get; }

    /// <summary>
    /// Called when the server is ready.
    /// </summary>
    void OnReady();
    
    /// <summary>
    /// Registers custom packets.
    /// </summary>
    /// <param name="registry"></param>
    void RegisterPackets(PacketRegistry registry);
    
    /// <summary>
    /// Registers custom entities.
    /// </summary>
    /// <param name="entityFactory">The entity factory.</param>
    void RegisterEntities(EntityFactory entityFactory);
    
    /// <summary>
    /// Creates an entity for a client.
    /// </summary>
    /// <param name="realm">
    /// The realm we're creating the entity in.
    /// </param>
    /// <param name="client">
    /// The client for whom we're creating the entity.
    /// </param>
    /// <returns>
    /// The created entity.
    /// </returns>
    BaseEntity CreateEntityForClient(Realm realm, Client client);
}