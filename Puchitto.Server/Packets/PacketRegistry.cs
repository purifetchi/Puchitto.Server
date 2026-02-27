using Puchitto.Server.Clients;

namespace Puchitto.Server.Packets;

public class PacketRegistry
{
    /// <summary>
    /// The dictionary of packet handlers.
    /// </summary>
    private readonly Dictionary<int, IPacketHandler> _handlers = new();

    /// <summary>
    /// Registers a new packer handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    public void RegisterHandler<TPacket>(Func<TPacket, Client, Task> handler)
        where TPacket : IPuchittoPacket, new()
    {
        var packetHandler = new SimplePacketHandler<TPacket>()
        {
            Handler = handler
        };
        
        var dummy = new TPacket();
        _handlers.Add(dummy.PacketId, packetHandler); 
    }

    /// <summary>
    /// Executes a packet handler.
    /// </summary>
    /// <param name="packetId">The packet ID.</param>
    /// <param name="payload">The packet payload.</param>
    /// <param name="client">The client.</param>
    public async Task ExecuteHandler(
        int packetId, 
        ArraySegment<byte> payload,
        Client client)
    {
        if (!_handlers.TryGetValue(packetId, out var handler))
        {
            return;
        }
        
        await handler.HandlePacket(payload, client);
    }
    
    /// <summary>
    /// Checks whether a given packet exists.
    /// </summary>
    /// <param name="packetId">The packet ID.</param>
    /// <returns>
    /// Whether the given packet exists.
    /// </returns>
    public bool PacketExists(int packetId)
    {
        return _handlers.ContainsKey(packetId);
    }
}