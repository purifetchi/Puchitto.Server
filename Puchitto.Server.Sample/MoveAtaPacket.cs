using System.Numerics;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Sample;

public struct MoveAtaPacket() : IPuchittoPacket
{
    public int PacketId { get; } = 101;

    public string Id { get; set; }
    
    public Vector3 To { get; set; }
    
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteString(Id);
        writer.WriteVector3(To);
    }

    public void Deserialize(ref NetworkReader reader)
    {
    }
}