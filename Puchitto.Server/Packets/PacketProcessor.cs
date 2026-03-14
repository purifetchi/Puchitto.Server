using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

/// <summary>
/// The packet processor.
/// </summary>
public class PacketProcessor
{
    /// <summary>
    /// The packet registry.
    /// </summary>
    public PacketRegistry Registry { get;  }

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<PacketProcessor> _logger;

    /// <summary>
    /// Constructs a new packet processor.
    /// </summary>
    /// <param name="registry">The associated registry.</param>
    /// <param name="logger">The logger.</param>
    public PacketProcessor(
        PacketRegistry registry,
        ILogger<PacketProcessor> logger)
    {
        Registry = registry;
        _logger = logger;
    }
    
    /// <summary>
    /// Processes incoming data.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="data">The data.</param>
    public async Task ProcessIncomingPacket(
        Client client,
        ArraySegment<byte> data)
    {
        const int maxPacketSize = 1024 * 10;
        
        var reader = new NetworkReader(data.Array, 0);
        var envelope = new PacketEnvelope(ref reader);

        // This client is doing some weird stuff, disconnect them.
        if (envelope.Size is > maxPacketSize or < 0)
        {
            // TODO: Reason.
            await client.Disconnect();
            _logger.LogError("Client {Id} sent us a packet with an invalid size! (Size was {Size})",
                client.Id,
                envelope.Size);
            return;
        }

        // The sequence number should never be negative.
        if (envelope.SequenceNumber < 0)
        {
            await client.Disconnect();
            _logger.LogError("Client {Id} sent us a packet with an invalid sequence number! (Sequence was {Seq})",
                client.Id,
                envelope.SequenceNumber);
            return;
        }

        // If the packet doesn't exist, we can safely tell the client to leave.
        if (!Registry.PacketExists(envelope.OpCode))
        {
            await client.Disconnect();
            _logger.LogError("Client {Id} sent us a packet for which we have no handler! (OpCode was {Op})",
                client.Id,
                envelope.OpCode);
            return;
        }
        
        var slice = data.Slice(PacketEnvelope.EnvelopeSize, envelope.Size);
        await Registry.ExecuteHandler(envelope.OpCode, slice, client);
    }
}