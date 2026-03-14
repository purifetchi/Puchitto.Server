using System.Numerics;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine.Clientbound;

public struct CreateEntityPacket() : IPuchittoPacket
{
    public int PacketId => (int)InternalPacketTypes.CreateEntity;

    /// <summary>
    /// The ID of the entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    
    /// <summary>
    /// The position of the entity.
    /// </summary>
    public Vector3 Position { get; set; }
    
    /// <summary>
    /// The rotation of the entity.
    /// </summary>
    public Quaternion Rotation { get; set; }
    
    /// <summary>
    /// The scale of the entity.
    /// </summary>
    public Vector3 Scale { get; set; }
    
    /// <summary>
    /// Is the given client the owner of the entity?
    /// </summary>
    public bool IsOwner { get; set; }
    
    /// <summary>
    /// The entity data.
    /// </summary>
    public string JsonEntityData { get; set; } = string.Empty;

    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteString(Id);
        writer.WriteString(EntityName);
        writer.WriteVector3(Position);
        writer.WriteQuaternion(Rotation);
        writer.WriteVector3(Scale);
        writer.WriteBoolean(IsOwner);
        writer.WriteString(JsonEntityData);
    }

    public void Deserialize(ref NetworkReader reader)
    {
    }
}