using Puchitto.Server.Packets.Serialization;
using Puchitto.Server.Packets.Serialization.Facades;

namespace Puchitto.Server.Packets.Engine.Bidirectional;

/// <summary>
/// A MiniAntics RPC call.
/// </summary>
public struct MiniAnticsRpcPacket : IPuchittoPacket
{
    /// <inheritdoc />
    public int PacketId => (int)InternalPacketTypes.MiniAnticsRpc;
    
    /// <summary>
    /// The ID of the object where the MiniAntics RPC is being invoked.
    /// </summary>
    public int ObjectId { get; set; }
    
    /// <summary>
    /// The name of the RPC.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The payload of the call.
    /// </summary>
    public ArraySegment<byte> Payload { get; set; }
    
    /// <inheritdoc />
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteInt32(ObjectId);
        writer.WriteString(Name);
    }
    
    /// <summary>
    /// Serializes the packet into a WriterFacade.
    /// </summary>
    /// <param name="writer">The WriterFacade.</param>
    public void Serialize(WriterFacade writer)
    {
        writer.WriteInt32(ObjectId);
        writer.WriteString(Name);
    }

    /// <inheritdoc />
    public void Deserialize(ref NetworkReader reader)
    {
        ObjectId = reader.ReadInt32();
        Name = reader.ReadString();
        // TODO: Payload
    }
}