using Puchitto.Server.Packets.Serialization;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Packets.Engine.Serverbound;

/// <summary>
/// The join packet sent by the client.
/// </summary>
public struct JoinPacket : IPuchittoPacket
{
    public int PacketId => (int)InternalPacketTypes.Join;
    
    public RealmLink? Link { get; set; }

    public JoinPacket()
    {
    }
    
    public void Serialize(ref NetworkWriter writer)
    {
    }

    public void Deserialize(ref NetworkReader reader)
    {
        Link = RealmLink.TryParse(reader.ReadString());
    }
}