using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Game.Entities.Scripting;
using Puchitto.Server.Networking;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Engine.Bidirectional;
using Puchitto.Server.Packets.Engine.Clientbound;
using Puchitto.Server.Packets.Engine.Serverbound;
using Puchitto.Server.Realms;
using Puchitto.Server.Scripting;

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
    public RealmManager RealmManager { get; }

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
    /// The base miniantics environment.
    /// </summary>
    private readonly MiniAnticsEnvironment _miniAnticsEnvironment;
    
    /// <summary>
    /// Constructs a new Puchitto server.
    /// </summary>
    /// <param name="config">The server config.</param>
    public PuchittoServer(PuchittoServerConfig config)
    {
        _config = config;
        _miniAnticsEnvironment = new();
        _rules = new TGameServerRules
        {
            PuchittoSystemsProvider = this
        };

        var loggingBuilder = config.LoggingBuilder ?? (opts =>
        {
            opts.AddProvider(NullLoggerProvider.Instance);
        });
        
        _loggerFactory = LoggerFactory.Create(loggingBuilder);
        _logger = MakeLogger<PuchittoServer<TGameServerRules>>();

        Registry = new PacketRegistry();
        _packetProcessor = new PacketProcessor(
            Registry,
            MakeLogger<PacketProcessor>()
        );
        
        _webSocketListener = new WebSocketListener(
            _config.Prefixes,
            MakeLogger<WebSocketListener>()
        );

        ClientManager = new ClientManager(
            _rules,
            _packetProcessor,
            MakeLogger<ClientManager>()
        );

        RealmManager = new RealmManager(
            this,
            _rules
        );

        RegisterInternalHandlers();
        SetupMiniAnticsEnvironment();
    }
    
    /// <summary>
    /// Hosts the game.
    /// </summary>
    public async Task Host()
    {
        await RealmManager.LoadRealms();
        
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
        Registry.RegisterHandler<MiniAnticsRpcPacket>(OnMiniAnticsRpc);
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

        // TODO: We shouldn't iterate the realms returned by the rules every time.
        var defaultRealm = _rules.GetRealmDefinitions()
            .First(r => r.IsDefault);
        var path = defaultRealm.RemotePackagePath ?? defaultRealm.LocalPackagePath;
        
        await client.SendData(new LoadPacket
        {
            LevelName = path
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
        
        if (newState == ClientState.Loaded)
        {
            await RealmManager.Default.SpawnPlayer(client, _rules);
        }
    }
    
    /// <summary>
    /// Executed when a client invokes a MiniAntics rpc.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="client">The client sending it.</param>
    private async Task OnMiniAnticsRpc(MiniAnticsRpcPacket packet, Client client)
    {
        _logger.LogInformation("Object {Id} just got a MiniAntics RPC {Name} from client {Client}",
            packet.ObjectId,
            packet.Name,
            client.Id);
        
        // Find the object
        // TODO: Make this client realm aware once we implement multi-realm simulations.
        var entity = RealmManager
            .Default
            .EntityManager
            .GetEntityById<BaseEntity>(packet.ObjectId);
        
        entity?.RunAntics(AnticsOn.Rpc, packet.Name);
    }
    
    /// <inheritdoc />
    public ILogger<T> MakeLogger<T>()
    {
        return _loggerFactory.CreateLogger<T>();
    }

    /// <summary>
    /// Constructs a new child environment.
    /// </summary>
    /// <returns>The child MiniAntics environment.</returns>
    public MiniAnticsEnvironment MakeChildEnvironment()
    {
        return new MiniAnticsEnvironment(_miniAnticsEnvironment);
    }

    /// <summary>
    /// Sets up the base MiniAntics environment.
    /// </summary>
    private void SetupMiniAnticsEnvironment()
    {
        _miniAnticsEnvironment.Set("+", (float a, float b) => a + b);
        _miniAnticsEnvironment.Set("-", (float a, float b) => a - b);
        _miniAnticsEnvironment.Set("/", (float a, float b) => a / b);
        _miniAnticsEnvironment.Set("*", (float a, float b) => a * b);

        _miniAnticsEnvironment.Set("not", (bool value) => !value);
        _miniAnticsEnvironment.Set("null?", (object? value) => value is null);
        _miniAnticsEnvironment.Set("equal?", (object a, object b) => a.Equals(b));
        _miniAnticsEnvironment.Set("different?", (object a, object b) => !a.Equals(b));
        
        _miniAnticsEnvironment.Set("print", (object value) =>
        {
            _logger.LogInformation($"[MiniAntics] {value}");
        });
        
        _miniAnticsEnvironment.Set("pass", () => { });
        _miniAnticsEnvironment.Set("progn", (params object[]? values) => values is { Length: > 0 } ? values[^1] : null);
    }
}