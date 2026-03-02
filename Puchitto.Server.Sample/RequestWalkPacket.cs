using System.Numerics;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Sample;

public struct RequestWalkPacket() : IPuchittoPacket
{
    public int PacketId { get; } = 100;
    
    public Vector3 To { get; set; }
    
    public void Serialize(ref NetworkWriter writer)
    {
        
    }

    public void Deserialize(ref NetworkReader reader)
    {
        To = reader.ReadVector3();
    }
}