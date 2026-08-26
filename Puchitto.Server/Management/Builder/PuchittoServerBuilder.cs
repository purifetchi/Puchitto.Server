using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Packets;
using Puchitto.Server.Realms;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Management.Builder;

public class PuchittoServerBuilder :
    IRulesBuilderStep,
    IEndpointBuilderStep,
    IPuchittoServerBuilder
{
    /// <summary>
    /// The game server rules.
    /// </summary>
    private IGameServerRules? _rules;

    /// <summary>
    /// The URLs to listen on.
    /// </summary>
    private string[]? _listenUrls;

    /// <summary>
    /// The server config.
    /// </summary>
    private readonly PuchittoServerConfig _config = new();

    /// <summary>
    /// The logging builder.
    /// </summary>
    private Action<ILoggingBuilder> _loggingBuilder = DefaultLoggingBuilder;

    /// <summary>
    /// Make the only constructor internal.
    /// </summary>
    internal PuchittoServerBuilder()
    {
        
    }
    
    /// <inheritdoc />
    public IEndpointBuilderStep UseRules(IGameServerRules rules)
    {
        _rules = rules;
        return this;
    }

    /// <inheritdoc />
    public IEndpointBuilderStep UseRules<T>() where T : IGameServerRules, new()
    {
        _rules = new T();
        return this;
    }

    /// <inheritdoc />
    public IPuchittoServerBuilder Listen(params string[] urls)
    {
        _listenUrls = urls;
        return this;
    }

    /// <inheritdoc />
    public IPuchittoServerBuilder Configure(Action<PuchittoServerConfig> configureAction)
    {
        configureAction(_config);
        return this;
    }

    /// <inheritdoc />
    public IPuchittoServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        _loggingBuilder = configureLogging;
        return this;
    }

    /// <inheritdoc />
    public PuchittoServer Build()
    {
        if (_rules is null)
        {
            throw new PuchittoBuilderException("No game rules specified, missing call to UseRules().");
        }

        if (_listenUrls is not { Length: > 0 })
        {
            throw new PuchittoBuilderException("Listen URLs must be specified, call Listen().");
        }

        var loggerFactory = LoggerFactory.Create(_loggingBuilder);

        var packetRegistry = new PacketRegistry();
        var entityFactory = new EntityFactory();
        var realmRegistry = new RealmRegistry();

        _rules.ConfigureRealms(realmRegistry);
        _rules.RegisterPackets(packetRegistry);
        _rules.RegisterEntities(entityFactory);
        
        Validate(realmRegistry);
        
        var server = new PuchittoServer(
            _listenUrls,
            _config,
            _rules,
            packetRegistry,
            entityFactory,
            realmRegistry,
            loggerFactory);
        
        _rules.Attach(server);
        _rules.OnReady();
        
        return server;
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    private void Validate(IRealmRegistry realmRegistry)
    {
        var realms = realmRegistry.GetRealmDefinitions()
            .ToList();

        if (realms.Count < 1)
        {
            throw new PuchittoBuilderException("No realms specified.");
        }

        var defaultRealmCount = realms.Count(r => r.Flags.HasFlag(RealmFlags.Default));
        switch (defaultRealmCount)
        {
            case < 1:
                throw new PuchittoBuilderException("No default realm specified.");
            case > 1:
                throw new PuchittoBuilderException("Multiple default realms specified.");
        }
    }

    /// <summary>
    /// The default logging builder.
    /// </summary>
    /// <param name="loggingBuilder">The logging builder.</param>
    private static void DefaultLoggingBuilder(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddProvider(NullLoggerProvider.Instance);
    }
}