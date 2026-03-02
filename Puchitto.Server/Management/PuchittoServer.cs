using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Level;
using Puchitto.Server.Networking;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Engine.Clientbound;
using Puchitto.Server.Packets.Engine.Serverbound;

namespace Puchitto.Server.Management;

/// <summary>
/// The Puchitto server instance.
/// </summary>
/// <typeparam name="TGameServerRules">The type of the game server rules.</typeparam>
public class PuchittoServer<TGameServerRules> : IPuchittoSystemsProvider
    where TGameServerRules : IGameServerRules, new()
{
    /// <summary>
    /// The entity manager.
    /// </summary>
    public EntityManager EntityManager { get; }

    /// <summary>
    /// The client manager.
    /// </summary>
    public ClientManager ClientManager { get; }

    /// <summary>
    /// The packet registry.
    /// </summary>
    public PacketRegistry Registry { get; }

    /// <summary>
    /// The current config.
    /// </summary>
    private readonly PuchittoServerConfig _config;

    /// <summary>
    /// Creates a new logger factory.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;
    
    /// <summary>
    /// The logger for this server.
    /// </summary>
    private readonly ILogger<PuchittoServer<TGameServerRules>> _logger;

    /// <summary>
    /// The web socket listener.
    /// </summary>
    private readonly WebSocketListener _webSocketListener;

    /// <summary>
    /// The packet processor.
    /// </summary>
    private readonly PacketProcessor _packetProcessor;

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
        _rules = new TGameServerRules
        {
            PuchittoSystemsProvider = this
        };

        var loggingBuilder = config.LoggingBuilder ?? (opts =>
        {
            opts.AddProvider(NullLoggerProvider.Instance);
        });
        
        _loggerFactory = LoggerFactory.Create(loggingBuilder);
        _logger = _loggerFactory.CreateLogger<PuchittoServer<TGameServerRules>>();

        Registry = new PacketRegistry();
        _packetProcessor = new PacketProcessor(Registry);
        
        _webSocketListener = new WebSocketListener(
            _config.Prefixes,
            _loggerFactory.CreateLogger<WebSocketListener>()
        );

        ClientManager = new ClientManager(
            _rules,
            _packetProcessor,
            _loggerFactory.CreateLogger<ClientManager>()
        );

        EntityManager = new EntityManager(
            ClientManager,
            _loggerFactory.CreateLogger<EntityManager>()
        );

        RegisterInternalHandlers();
    }
    
    /// <summary>
    /// Hosts the game.
    /// </summary>
    public async Task Host()
    {
        _webSocketListener.OnClientConnected =
            connection => ClientManager.AcceptConnection(connection);
        
        await _webSocketListener.Listen();
    }

    /// <summary>
    /// Registers the internal packet handlers.
    /// </summary>
    private void RegisterInternalHandlers()
    {
        _rules.RegisterPackets(Registry);
        
        Registry.RegisterHandler<JoinPacket>(OnJoin);
        Registry.RegisterHandler<LoadStatePacket>(OnLoadState);
    }

    /// <summary>
    /// Executed when the client sends us a join packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="client">The client sending it.</param>
    private async Task OnJoin(JoinPacket packet, Client client)
    {
        _logger.LogInformation("Client {Id} sent us a join packet!", client.Id);
        client.SetState(ClientState.Connecting);

        await client.SendData(new LoadPacket
        {
            LevelName = _rules.GetPackagePath()
        });
    }
    
    /// <summary>
    /// Executed when the client sends a load state packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="client">The client sending it.</param>
    private async Task OnLoadState(LoadStatePacket packet, Client client)
    {
        var newState = packet.State is LoadState.Started
            ? ClientState.Loading
            : ClientState.Loaded;
        
        client.SetState(newState);
        _logger.LogInformation("Client {ClientName} is now in state {State}", client.Id, newState);
        
        // TODO: Move this to a more sensible location.
        if (newState == ClientState.Loaded)
        {
            await EntityManager.SpawnMissingEntitiesFor(client);
            
            var entity = _rules.CreateEntityForClient();
            entity.Owner = client;
            await EntityManager.AddAndSpawnForEveryone(entity);    
        }
    }
}