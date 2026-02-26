using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

/// <summary>
/// The packet envelope.
/// </summary>
public readonly record struct PacketEnvelope(int SequenceNumber, int OpCode, int Size)
{
    /// <summary>
    /// The sequence number, always monotonically rising.
    /// </summary>
    public int SequenceNumber { get; } = SequenceNumber;

    /// <summary>
    /// The OpCode of the packet, denoting the type.
    /// </summary>
    public int OpCode { get; } = OpCode;

    /// <summary>
    /// The size of the payload.
    /// </summary>
    public int Size { get; } = Size;

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