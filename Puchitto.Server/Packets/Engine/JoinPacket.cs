using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine;

/// <summary>
/// The join packet sent by the client.
/// </summary>
public struct JoinPacket : IPuchittoPacket
{
    public int PacketId { get; } = 2;
 
    public JoinPacket()
    {
    }
    
    public void Serialize(ref NetworkWriter writer)
    {
    }

    public void Deserialize(ref NetworkReader reader)
    {
    }
}