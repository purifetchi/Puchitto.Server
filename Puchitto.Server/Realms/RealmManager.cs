using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;
using Puchitto.Server.Data.Alf;
using Puchitto.Server.Game;
using Puchitto.Server.Management;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

/// <summary>
/// Responsible for loading and managing realms.
/// </summary>
public class RealmManager
{
    /// <summary>
    /// Gets the default realm.
    /// </summary>
    public Realm Default { get; private set; } = null!;
    
    private readonly ConcurrentDictionary<string, RealmSlot> _realms = new();
    private readonly IPuchittoSystemsProvider _systemsProvider;
    
    /// <summary>
    /// Constructs a new realm manager.
    /// </summary>
    /// <param name="puchittoSystemsProvider"></param>
    public RealmManager(IPuchittoSystemsProvider puchittoSystemsProvider)
    {
        _systemsProvider = puchittoSystemsProvider;
    }

    /// <summary>
    /// Loads all the realms in parallel.
    /// </summary>
    public async Task LoadRealms()
    {
        var defs = _systemsProvider.RealmRegistry.GetRealmDefinitions();
        var realmTasks = defs
            .Where(d => d.Flags.HasFlag(RealmFlags.Persistent))
            .Select(BeginRealmLoad);
        
        await Task.WhenAll(realmTasks);

        Default = _realms.First(r => r.Value.Realm.Flags.HasFlag(RealmFlags.Default))
            .Value
            .Realm;
        
        var logger = _systemsProvider.LoggerFactory.CreateLogger<RealmManager>();
        logger.LogInformation("Loaded {Count} realm(s).", _realms.Count);
    }

    /// <summary>
    /// Gets or begins loading a realm.
    /// </summary>
    /// <param name="name">The name of the realm.</param>
    /// <returns>The realm itself.</returns>
    public async Task<Realm> GetOrLoadRealm(string name)
    {
        // TODO: Check if we have this realm loaded.
        return _realms.Values.First(r => r.Realm.Name == name).Realm;
    }

    /// <summary>
    /// Admits a client to a given realm, specified by a link.
    /// </summary>
    /// <param name="client">The client to admit.</param>
    /// <param name="link">The realm.</param>
    public async Task TransferClient(Client client, RealmLink link)
    {
        var source = client.CurrentRealm;
        if (!client.TryBeginRealmTransfer())
        {
            return;
        }

        var destination = await GetOrLoadRealm(link.RealmName);
        if (!destination.TryReserveSlot(client))
        {
            return;
        }
        
        // If we have no current realm, we can admit to the new realm already. 
        if (source == null)
        {
            destination.PostAdmit(client, link.QueryPath);
            return;
        }
        
        // Otherwise dispatch on the source realm's thread that we want to move to the new realm.
        source.DispatchOnRealmThread(async realm =>
        {
            await realm.RemoveClient(client);
            destination.PostAdmit(client, link.QueryPath);
        });
    }

    /// <summary>
    /// Begins loading a singular realm.
    /// </summary>
    /// <param name="definition">The realm's definition</param>
    private async Task BeginRealmLoad(RealmDefinition definition)
    {
        var realm = await CreateRealm(definition);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _ = Task.Run(async () =>
        {
            await RealmTickLoop(realm, cancellationToken);
        }, cancellationToken);
        
        _realms.TryAdd(definition.Name, new RealmSlot(RealmState.Loaded, realm, cts));
    }

    /// <summary>
    /// Starts ticking a realm.
    /// </summary>
    /// <param name="realm">The realm.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RealmTickLoop(Realm realm, CancellationToken cancellationToken)
    {
        const float tps = 20.0f;
        const float delay = 1 / tps;

        var timeSpan = TimeSpan.FromSeconds(delay);
        while (!cancellationToken.IsCancellationRequested)
        {
            await realm.Tick();
            
            // TODO: This should be configurable.
            await Task.Delay(timeSpan, cancellationToken);
        }
    }
    
    /// <summary>
    /// Loads a single realm from the realm definition.
    /// </summary>
    private async Task<Realm> CreateRealm(RealmDefinition definition)
    {
        const string levelFilePath = "\\level.json";
        using var package = new AlfPackage(definition.LocalPackagePath);
        
        // Get the level lump.
        var level = package.Lumps
            .First(l => l.Path == levelFilePath);

        await using var jsonStream = package.GetStream(level);
        var levelData = await JsonSerializer.DeserializeAsync<Level>(jsonStream);
        if (levelData == null)
        {
            // TODO
            throw new InvalidOperationException("Tried to load realm with a broken level.json file.");
        }
        
        var realm = new Realm(_systemsProvider, levelData, definition);
        return realm;
    }
}