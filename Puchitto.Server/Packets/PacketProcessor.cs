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
    /// Constructs a new packet processor.
    /// </summary>
    /// <param name="registry">The associated registry.</param>
    public PacketProcessor(PacketRegistry registry)
    {
        Registry = registry;
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
            return;
        }

        // The sequence number should never be negative.
        if (envelope.SequenceNumber < 0)
        {
            await client.Disconnect();
            return;
        }

        // If the packet doesn't exist, we can safely tell the client to leave.
        if (!Registry.PacketExists(envelope.OpCode))
        {
            await client.Disconnect();
            return;
        }

        var slice = data.Slice(PacketEnvelope.EnvelopeSize, envelope.Size);
    }
}