using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Packets;

namespace Puchitto.Server.Management;

/// <summary>
/// Provides access to various Puchitto subsystems.
/// </summary>
public interface IPuchittoSystemsProvider
{
    /// <summary>
    /// The entity manager.
    /// </summary>
    EntityManager EntityManager { get; }
    
    /// <summary>
    /// The client manager.
    /// </summary>
    ClientManager ClientManager { get; }
    
    /// <summary>
    /// The packet registry.
    /// </summary>
    PacketRegistry Registry { get; }
}