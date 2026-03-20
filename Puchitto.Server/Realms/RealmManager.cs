using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
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
    /// The list of loaded realms.
    /// </summary>
    public IReadOnlyList<Realm> Realms => _realms;

    /// <summary>
    /// Gets the default realm.
    /// </summary>
    public Realm Default { get; private set; } = null!;
    
    private List<Realm> _realms = [];
    private readonly IPuchittoSystemsProvider _systemsProvider;
    private readonly IGameServerRules _rules;
    
    /// <summary>
    /// Constructs a new realm manager.
    /// </summary>
    /// <param name="puchittoSystemsProvider"></param>
    public RealmManager(
        IPuchittoSystemsProvider puchittoSystemsProvider,
        IGameServerRules rules)
    {
        _systemsProvider = puchittoSystemsProvider;
        _rules = rules;
    }

    /// <summary>
    /// Loads all the realms in parallel.
    /// </summary>
    public async Task LoadRealms()
    {
        var defs = _rules.GetRealmDefinitions();
        var realmTasks = defs.Select(CreateRealm);
        var realms = await Task.WhenAll(realmTasks);

        _realms = realms.ToList();
        Default = _realms.First(r => r.IsDefault);
        
        var logger = _systemsProvider.MakeLogger<RealmManager>();
        logger.LogInformation("Loaded {Count} realm(s).", _realms.Count);
    }

    /// <summary>
    /// Loads a single realm from the realm definition.
    /// </summary>
    private async Task<Realm> CreateRealm(RealmDefinition definition)
    {
        using var package = new AlfPackage(definition.LocalPackagePath);
        
        // Get the level lump.
        var level = package.Lumps
            .First(l => l.Path == "\\level.json");

        await using var jsonStream = package.GetStream(level);
        var levelData = await JsonSerializer.DeserializeAsync<Level>(jsonStream);
        if (levelData == null)
        {
            // TODO
            throw new InvalidOperationException("Tried to load realm with a broken level.json file.");
        }
        
        var realm = new Realm(_systemsProvider, levelData, definition.IsDefault);
        return realm;
    }
}