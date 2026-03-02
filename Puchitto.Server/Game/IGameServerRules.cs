using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets;

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
    /// Registers custom packets.
    /// </summary>
    /// <param name="registry"></param>
    void RegisterPackets(PacketRegistry registry);
    
    /// <summary>
    /// Gets the path to the package.
    /// </summary>
    /// <returns>
    /// The path to the package.
    /// </returns>
    string GetPackagePath();
    
    /// <summary>
    /// Creates an entity for a client.
    /// </summary>
    /// <returns>
    /// The created entity.
    /// </returns>
    BaseEntity CreateEntityForClient();
}