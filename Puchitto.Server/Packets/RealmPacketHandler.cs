using Puchitto.Server.Clients;
using Puchitto.Server.Packets.Serialization;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Packets;

/// <summary>
/// Handles a realm packet.
/// </summary>
/// <typeparam name="TPacket">The type of the packet.</typeparam>
public class RealmPacketHandler<TPacket> : IPacketHandler
    where TPacket : struct, IPuchittoPacket
{
    /// <inheritdoc />
    public PacketDispatchType DispatchType => PacketDispatchType.Realm;
    
    /// <summary>
    /// The actual handler.
    /// </summary>
    public Func<TPacket, Realm, Client, Task> Handler { get; set; } = null!;
    
    /// <inheritdoc />
    public async Task HandlePacket(ArraySegment<byte> data, Client client)
    {
        var reader = new NetworkReader(data, 0);
        var packet = new TPacket();
        
        packet.Deserialize(ref reader);
        
        await Handler(packet, client.CurrentRealm!, client);
    }
}