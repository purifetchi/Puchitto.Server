using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine.Clientbound;

/// <summary>
/// Sent whenever we want to remove an entity.
/// </summary>
public struct RemoveEntityPacket() : IPuchittoPacket
{
    public int PacketId => (int)InternalPacketTypes.RemoveEntity;

    /// <summary>
    /// The ID of the entity.
    /// </summary>
    public int Id { get; set; }
    
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteInt32(Id);
    }

    public void Deserialize(ref NetworkReader reader)
    {
    }
}