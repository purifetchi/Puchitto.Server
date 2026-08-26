using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

/// <summary>
/// Defines a single serializable Puchitto packet.
/// </summary>
public interface IPuchittoPacket
{
    /// <summary>
    /// The ID of the Puchitto packet.
    /// </summary>
    static abstract int PacketId { get; }
    
    /// <summary>
    /// Serializes this packet into the network writer.
    /// </summary>
    /// <param name="writer">The network writer.</param>
    void Serialize(ref NetworkWriter writer);
    
    /// <summary>
    /// Serializes the packet into the network reader.
    /// </summary>
    /// <param name="reader">The network reader.</param>
    void Deserialize(ref NetworkReader reader);
}