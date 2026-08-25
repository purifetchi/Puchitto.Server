using Puchitto.Server.Packets;

namespace Puchitto.Server.Clients;

/// <summary>
/// Provides a grouping of clients.
/// </summary>
public interface IClientGroupProvider
{
    /// <summary>
    /// The clients this grouping has.
    /// </summary>
    IReadOnlyCollection<Client> Clients { get; }

    /// <summary>
    /// Sends a packet to all the clients within this group.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <param name="excluding">The client to exclude from sending to.</param>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    Task SendToClients<TPacket>(TPacket packet, Client? excluding = null)
        where TPacket: struct, IPuchittoPacket;
}