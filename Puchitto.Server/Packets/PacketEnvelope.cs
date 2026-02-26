using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

/// <summary>
/// The packet envelope.
/// </summary>
public readonly record struct PacketEnvelope
{
    /// <summary>
    /// The size of the packet envelope.
    /// </summary>
    public const int EnvelopeSize = sizeof(int) * 3;
    
    /// <summary>
    /// The sequence number, always monotonically rising.
    /// </summary>
    public int SequenceNumber { get; }

    /// <summary>
    /// The OpCode of the packet, denoting the type.
    /// </summary>
    public int OpCode { get; }

    /// <summary>
    /// The size of the payload.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Constructs a new packet envelope.
    /// </summary>
    public PacketEnvelope(int sequenceNumber, int opCode, int size)
    {
        SequenceNumber = sequenceNumber;
        OpCode = opCode;
        Size = size;
    }
    
    /// <summary>
    /// Reads a packet envelope from the network reader.
    /// </summary>
    /// <param name="reader">The network reader.</param>
    public PacketEnvelope(ref NetworkReader reader)
    {
        SequenceNumber = reader.ReadInt32();
        OpCode = reader.ReadInt32();
        Size = reader.ReadInt32();
    }
    
    /// <summary>
    /// Serializes the envelope.
    /// </summary>
    /// <param name="writer">The network writer.</param>
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteInt32(SequenceNumber);
        writer.WriteInt32(OpCode);
        writer.WriteInt32(Size);
    }
}