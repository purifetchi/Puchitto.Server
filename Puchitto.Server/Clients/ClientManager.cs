using Microsoft.Extensions.Logging;

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
    /// The client lock.
    /// </summary>
    private readonly Lock _clientLock = new(); 

    /// <summary>
    /// Constructs a new client manager.
    /// </summary>
    /// <param name="logger">
    /// The logger.
    /// </param>
    public ClientManager(ILogger<ClientManager> logger)
    {
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
    }
}