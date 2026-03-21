using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Packets;
using Puchitto.Server.Realms;
using Puchitto.Server.Scripting;

namespace Puchitto.Server.Management;

/// <summary>
/// Provides access to various Puchitto subsystems.
/// </summary>
public interface IPuchittoSystemsProvider
{
    /// <summary>
    /// The realm manager.
    /// </summary>
    RealmManager RealmManager { get; }
    
    /// <summary>
    /// The client manager.
    /// </summary>
    ClientManager ClientManager { get; }
    
    /// <summary>
    /// The packet registry.
    /// </summary>
    PacketRegistry Registry { get; }
    
    /// <summary>
    /// The entity factory.
    /// </summary>
    EntityFactory EntityFactory { get; }

    /// <summary>
    /// Constructs a logger for a given type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The created logger.</returns>
    ILogger<T> MakeLogger<T>();

    /// <summary>
    /// Makes the child environment from the base environment.
    /// </summary>
    /// <returns>The child environment.</returns>
    MiniAnticsEnvironment MakeChildEnvironment();
}