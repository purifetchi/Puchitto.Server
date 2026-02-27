using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine;

public struct LoadPacket : IPuchittoPacket
{
    public int PacketId { get; } = 3;
    
    public string LevelName { get; set; }

    public LoadPacket()
    {
        LevelName = "";
    }
    
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteString(LevelName);
    }

    public void Deserialize(ref NetworkReader reader)
    {
        LevelName = reader.ReadString();
    }
}