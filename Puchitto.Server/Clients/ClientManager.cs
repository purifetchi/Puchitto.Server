using Microsoft.Extensions.Logging;
using Puchitto.Server.Game;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Engine;

namespace Puchitto.Server.Clients;

public class ClientManager
{
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
    /// The client lock.
    /// </summary>
    private readonly Lock _clientLock = new(); 

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
    /// Accepts a client connection.
    /// </summary>
    /// <param name="connection">The client connection.</param>
    public async Task AcceptConnection(ClientConnection connection)
    {
        var client = new Client(Guid.NewGuid(), connection);

        lock (_clientLock)
        {
            _clients.Add(client);
        }

        client.Connection.OnIncomingMessage = async (data) =>
            await _packetProcessor.ProcessIncomingPacket(client, data);

        _ = Task.Run(async () =>
        {
            await client.Connection.PumpMessages(CancellationToken.None);
        });

        await SendHandshake(client);
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