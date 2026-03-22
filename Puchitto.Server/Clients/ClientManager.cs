using Microsoft.Extensions.Logging;
using Puchitto.Server.Game;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Engine.Bidirectional;
using Puchitto.Server.Packets.Engine.Clientbound;

namespace Puchitto.Server.Clients;

/// <summary>
/// The base manager for clients.
/// </summary>
public class ClientManager
{
    /// <summary>
    /// The heartbeat interval.
    /// </summary>
    public const int HeartbeatIntervalInSeconds = 5;
    
    /// <summary>
    /// The delegate for when a client connects.
    /// </summary>
    public delegate Task ClientConnectedEvent(Client client);
    
    /// <summary>
    /// The delegate for when a client disconnects.
    /// </summary>
    public delegate Task ClientDisconnectedEvent(Client client);
    
    /// <summary>
    /// Invoked when a client connects.
    /// </summary>
    public event ClientConnectedEvent? OnClientConnected;
    
    /// <summary>
    /// Invoked when a client disconnects.
    /// </summary>
    public event ClientDisconnectedEvent? OnClientDisconnected;
    
    /// <summary>
    /// The list of present clients.
    /// </summary>
    private readonly List<Client> _clients = [];
    
    /// <summary>
    /// The logger for the 
    /// </summary>
    private readonly ILogger<ClientManager> _logger;

    /// <summary>
    /// The packet processor.
    /// </summary>
    private readonly PacketProcessor _packetProcessor;
    
    /// <summary>
    /// The game server rules.
    /// </summary>
    private readonly IGameServerRules _gameServerRules;
    
    /// <summary>
    /// The client semaphore.
    /// </summary>
    private readonly SemaphoreSlim _clientSemaphore = new(1, 1);
    
    /// <summary>
    /// Gets all the clients.
    /// </summary>
    public IReadOnlyList<Client> Clients => _clients;

    /// <summary>
    /// Constructs a new client manager.
    /// </summary>
    /// <param name="gameServerRules">
    /// The game server rules.
    /// </param>
    /// <param name="packetProcessor">
    /// The packet processor.
    /// </param>
    /// <param name="logger">
    /// The logger.
    /// </param>
    public ClientManager(
        IGameServerRules gameServerRules,
        PacketProcessor packetProcessor,
        ILogger<ClientManager> logger)
    {
        _gameServerRules = gameServerRules;
        _packetProcessor = packetProcessor;
        _logger = logger;
    }

    /// <summary>
    /// Heartbeats all the clients.
    /// </summary>
    public async Task HeartbeatClients()
    {
        var removedClients = new Queue<Client>();
        
        await _clientSemaphore.WaitAsync();
        var now = DateTimeOffset.UtcNow;
        foreach (var client in _clients)
        {
            var diff = now - client.LastKeepAliveReceived;
            
            // We can miss like two keepalives. 
            if (diff.TotalSeconds >= HeartbeatIntervalInSeconds * 2.5)
            {
                _logger.LogWarning("Client {Id} dropped two consecutive KeepAlive packets.", client.Id);
                removedClients.Enqueue(client);
                continue;
            }
            
            await client.SendData(new KeepAlivePacket());
        }
        
        _clientSemaphore.Release();

        while (removedClients.TryDequeue(out var client))
        {
            await client.Disconnect();
        }
    }

    /// <summary>
    /// Accepts a client connection.
    /// </summary>
    /// <param name="connection">The client connection.</param>
    public async Task AcceptConnection(ClientConnection connection)
    {
        var client = new Client(Guid.NewGuid(), connection);

        await _clientSemaphore.WaitAsync();
        _clients.Add(client);
        _clientSemaphore.Release();

        client.Connection.OnIncomingMessage = async (data) =>
            await _packetProcessor.ProcessIncomingPacket(client, data);

        client.Connection.OnConnectionClosed = async () =>
            await DisconnectClient(client);
        
        _ = Task.Run(async () =>
        {
            await client.Connection.PumpMessages(CancellationToken.None);
        });

        await SendHandshake(client);
    }

    /// <summary>
    /// Forget about the client.
    /// </summary>
    /// <param name="client">The client.</param>
    private async Task DisconnectClient(Client client)
    {
        _logger.LogInformation("Disconnecting client {Id}", client.Id);
        
        client.SetState(ClientState.Disconnected);
        if (OnClientDisconnected is not null)
        {
            await OnClientDisconnected.Invoke(client);
        }

        await _clientSemaphore.WaitAsync();
        _clients.Remove(client);
        _clientSemaphore.Release();
    }
    
    /// <summary>
    /// Sends the handshake to the client.
    /// </summary>
    /// <param name="client">The handshake.</param>
    private async Task SendHandshake(Client client)
    {
        _logger.LogInformation("Sending handshake to client of ID {Id}", client.Id);

        var hello = new HelloPacket("Puchitto.Server", _gameServerRules.Name);
        await client.SendData(hello);
    }
}