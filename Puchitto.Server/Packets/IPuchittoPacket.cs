using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

public interface IPuchittoPacket
{
    int PacketId { get; }
    
    void Serialize(ref NetworkWriter writer);
    void Deserialize();
}