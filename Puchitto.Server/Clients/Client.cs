using System.Buffers;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Clients;

/// <summary>
/// A Puchitto client.
/// </summary>
public class Client
{
    /// <summary>
    /// The ID of this client.
    /// </summary>
    public Guid Id { get; }
    
    /// <summary>
    /// The connection for this client.
    /// </summary>
    public ClientConnection Connection { get; }
    
    /// <summary>
    /// The last sent sequence id.
    /// </summary>
    private int _lastSentSequenceId;
    
    public Client(
        Guid id,
        ClientConnection connection)
    {
        Console.WriteLine($"Creating new client...");
        Id = id;
        Connection = connection;
    }

    /// <summary>
    /// Sends a packet to this client.
    /// </summary>
    /// <param name="data">
    /// The packet data.
    /// </param>
    /// <typeparam name="TPacket">
    /// The type of the packet.
    /// </typeparam>
    public async Task SendData<TPacket>(TPacket data)
        where TPacket : struct, IPuchittoPacket
    {
        const int envelopeSize = sizeof(int) * 3;
        
        // 10KiB by default.
        const int bufferSizeInBytes = 1024 * 10;
        
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSizeInBytes);

        // Skip over the size of the envelope to save space for it.
        var writer = new NetworkWriter(buffer, envelopeSize);
        data.Serialize(ref writer);

        var sequenceId = Interlocked.Increment(ref _lastSentSequenceId);
        var packetId = data.PacketId;
        var length = writer.Position;
        var envelope = new PacketEnvelope(sequenceId, packetId, length - envelopeSize);
        
        var envelopeWriter = new NetworkWriter(buffer, 0);
        envelope.Serialize(ref envelopeWriter);

        await Connection.SendBuffer(new ArraySegment<byte>(buffer, 0, length));
        
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public async Task Disconnect()
    {
        
    }
}