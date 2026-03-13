using Puchitto.Server.Packets.Serialization;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Packets.Engine.Serverbound;

public struct LoadStatePacket() : IPuchittoPacket
{
    public int PacketId { get; } = 4;
    
    public LoadState State { get; set; } = LoadState.Started;

    public void Serialize(ref NetworkWriter writer)
    {
    }

    public void Deserialize(ref NetworkReader reader)
    {
        State = (LoadState)reader.ReadInt32();
    }
}