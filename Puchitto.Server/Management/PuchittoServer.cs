using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Networking;

namespace Puchitto.Server.Management;

/// <summary>
/// The Puchitto server instance.
/// </summary>
/// <typeparam name="TGameServerRules">The type of the game server rules.</typeparam>
public class PuchittoServer<TGameServerRules>
    where TGameServerRules : IGameServerRules, new()
{
    /// <summary>
    /// The current config.
    /// </summary>
    private readonly PuchittoServerConfig _config;

    /// <summary>
    /// Creates a new logger factory.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// The web socket listener.
    /// </summary>
    private readonly WebSocketListener _webSocketListener;
    
    /// <summary>
    /// The client manager.
    /// </summary>
    private readonly ClientManager _clientManager;
    
    /// <summary>
    /// The game server rules.
    /// </summary>
    private readonly TGameServerRules _rules;
    
    /// <summary>
    /// Constructs a new Puchitto server.
    /// </summary>
    /// <param name="config">The server config.</param>
    public PuchittoServer(PuchittoServerConfig config)
    {
        _config = config;
        _rules = new TGameServerRules();

        var loggingBuilder = config.LoggingBuilder ?? (opts =>
        {
            opts.AddProvider(NullLoggerProvider.Instance);
        });
        
        _loggerFactory = LoggerFactory.Create(loggingBuilder);
        
        _webSocketListener = new WebSocketListener(
            _config.Prefixes,
            _loggerFactory.CreateLogger<WebSocketListener>()
        );

        _clientManager = new ClientManager(
            _rules,
            _loggerFactory.CreateLogger<ClientManager>()
        );
    }
    
    /// <summary>
    /// Hosts the game.
    /// </summary>
    public async Task Host()
    {
        _webSocketListener.OnClientConnected =
            connection => _clientManager.AcceptConnection(connection);
        
        await _webSocketListener.Listen();
    }
}