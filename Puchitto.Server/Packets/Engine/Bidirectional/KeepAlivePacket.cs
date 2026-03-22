using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine.Bidirectional;

/// <summary>
/// A packet sent by the client and server to keep the connection alive.
/// </summary>
public struct KeepAlivePacket() : IPuchittoPacket
{
    /// <inheritdoc />
    public int PacketId { get; } = (int)InternalPacketTypes.KeepAlive;
    
    /// <inheritdoc />
    public void Serialize(ref NetworkWriter writer)
    {
    }

    /// <inheritdoc />
    public void Deserialize(ref NetworkReader reader)
    {
    }
}