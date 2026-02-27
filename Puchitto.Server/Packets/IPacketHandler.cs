using Puchitto.Server.Clients;

namespace Puchitto.Server.Packets;

/// <summary>
/// A packet handler.
/// </summary>
public interface IPacketHandler
{
    /// <summary>
    /// Handles a packet.
    /// </summary>
    /// <param name="data">
    /// The incoming data.
    /// </param>
    /// <param name="client">
    /// The client which sent this packet.
    /// </param>
    Task HandlePacket(ArraySegment<byte> data, Client client);
}