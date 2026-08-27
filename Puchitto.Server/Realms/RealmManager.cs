using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;
using Puchitto.Server.Data.Alf;
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
    
    private readonly ConcurrentDictionary<string, RealmSlot> _realmSlots = new();
    private readonly IPuchittoSystemsProvider _systemsProvider;
    
    private readonly ILogger<RealmManager> _logger;
    
    /// <summary>
    /// Constructs a new realm manager.
    /// </summary>
    /// <param name="puchittoSystemsProvider"></param>
    public RealmManager(IPuchittoSystemsProvider puchittoSystemsProvider)
    {
        _systemsProvider = puchittoSystemsProvider;
        _logger = _systemsProvider.LoggerFactory.CreateLogger<RealmManager>();
    }

    /// <summary>
    /// Loads all the realms in parallel.
    /// </summary>
    public async Task LoadRealms()
    {
        var defs = _systemsProvider.RealmRegistry.GetRealmDefinitions();
        var realmTasks = defs
            .Where(d => d.Flags.HasFlag(RealmFlags.Persistent))
            .Select(d => d.Name)
            .Select(GetOrLoadRealm)
            .ToList();
        
        await Task.WhenAll(realmTasks);

        Default = realmTasks
            .Select(t => t.GetAwaiter().GetResult())
            .First(t => t.Flags == RealmFlags.Default);
        
        _logger.LogInformation("Loaded {Count} realm(s).", _realmSlots.Count);
    }

    /// <summary>
    /// Gets or begins loading a realm.
    /// </summary>
    /// <param name="name">The name of the realm.</param>
    /// <returns>The realm itself.</returns>
    public async Task<Realm> GetOrLoadRealm(string name)
    {
        // If we have a realm, return the loading task.
        if (_realmSlots.TryGetValue(name, out var realmSlot))
        {
            try
            {
                return await realmSlot.RealmTask.Value;
            }
            catch
            {
                _realmSlots.TryRemove(name, out _);
                throw;
            }
        }

        var definition = _systemsProvider.RealmRegistry.GetDefinitionForRealm(name);
        if (definition is null)
        {
            // TODO: Proper exception.
            throw new InvalidOperationException();
        }
        
        // Construct a new slot and try to add it.
        var newSlot = _realmSlots.GetOrAdd(name, _ => new RealmSlot()
        {
            State = RealmState.Loading,
            RealmTask = new Lazy<Task<Realm>>(() => BeginRealmLoad(definition))
        });
        
        try
        {
            return await newSlot.RealmTask.Value;
        }
        catch
        {
            _realmSlots.TryRemove(name, out _);
            throw;
        }
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
    private async Task<Realm> BeginRealmLoad(RealmDefinition definition)
    {
        var sw = new Stopwatch();
        sw.Start();
        
        var realm = await CreateRealm(definition);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var tickingTask = Task.Run(async () =>
        {
            await RealmTickLoop(realm, cancellationToken);
        }, cancellationToken);

        if (!_realmSlots.TryGetValue(realm.Name, out var realmSlot))
        {
            // What had just transpired??
            throw new InvalidOperationException("Realm loaded but has no realm slot? Huh?");
        }

        realmSlot.RealmTickingTask = tickingTask;
        realmSlot.RealmTickCancellation = cts;
        realmSlot.State = RealmState.Loaded;
        
        sw.Stop();
        
        _logger.LogInformation("Loaded realm {Name} in {Time} seconds.",
            realm.Name,
            sw.Elapsed.TotalSeconds);
        
        return realm;
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