using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets;
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
    /// Gets the list of realm definitions.
    /// </summary>
    /// <returns>The list of realm definitions for this game.</returns>
    IReadOnlyList<RealmDefinition> GetRealmDefinitions();

    /// <summary>
    /// Registers custom packets.
    /// </summary>
    /// <param name="registry"></param>
    void RegisterPackets(PacketRegistry registry);
    
    /// <summary>
    /// Creates an entity for a client.
    /// </summary>
    /// <returns>
    /// The created entity.
    /// </returns>
    BaseEntity CreateEntityForClient();
}