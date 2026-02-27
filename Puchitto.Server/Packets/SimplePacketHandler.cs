using Puchitto.Server.Clients;
using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets;

/// <summary>
/// The simple packet handler.
/// </summary>
/// <typeparam name="TPacket">
/// The type of the packet.
/// </typeparam>
public class SimplePacketHandler<TPacket> : IPacketHandler
    where TPacket : IPuchittoPacket, new()
{
    /// <summary>
    /// The actual handler.
    /// </summary>
    public Func<TPacket, Client, Task> Handler { get; set; } = null!;
    
    /// <inheritdoc />
    public async Task HandlePacket(ArraySegment<byte> data, Client client)
    {
        var reader = new NetworkReader(data, 0);
        var packet = new TPacket();
        
        packet.Deserialize(ref reader);
        
        await Handler(packet, client);
    }
}