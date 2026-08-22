using System.Threading.Channels;
using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets.Engine.Clientbound;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

/// <summary>
/// A single realm within a Puchitto server.
/// </summary>
public class Realm
{
    /// <summary>
    /// The type that is used as a thread action callback dispatch.
    /// </summary>
    public delegate Task RealmThreadActionCallback(Realm realm);
    
    /// <summary>
    /// The name of this realm.
    /// </summary>
    public string Name => _definition.Name;

    /// <summary>
    /// The flags for this realm.
    /// </summary>
    public RealmFlags Flags => _definition.Flags;

    /// <summary>
    /// The entity manager for this realm.
    /// </summary>
    public EntityManager EntityManager { get; }
    
    /// <summary>
    /// The ID allocator for this realm.
    /// </summary>
    public EntityIdAllocator IdAllocator { get; }
    
    /// <summary>
    /// The puchitto systems provider.
    /// </summary>
    public IPuchittoSystemsProvider SystemsProvider { get; }

    /// <summary>
    /// The delegate for when a client joins this realm.
    /// </summary>
    public delegate Task ClientJoinedRealmEvent(Client client);
    
    /// <summary>
    /// The delegate for when a client leaves this realm.
    /// </summary>
    public delegate Task ClientLeftRealmEvent(Client client);
    
    /// <summary>
    /// Invoked when a client joins this realm.
    /// </summary>
    public event ClientJoinedRealmEvent? OnClientJoinedRealm;
    
    /// <summary>
    /// Invoked when a client leaves this realm.
    /// </summary>
    public event ClientLeftRealmEvent? OnClientLeftRealm;

    /// <summary>
    /// The definition of this realm.
    /// </summary>
    private readonly RealmDefinition _definition;

    /// <summary>
    /// The thread executor channel.
    /// </summary>
    private readonly Channel<RealmThreadActionCallback> _threadExecutorChannel;

    /// <summary>
    /// Constructs a new Realm.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The systems provider.
    /// </param>
    public Realm(
        IPuchittoSystemsProvider puchittoSystemsProvider,
        Level levelDefinition,
        RealmDefinition definition)
    {
        _definition = definition;
        
        SystemsProvider = puchittoSystemsProvider;
        EntityManager = new EntityManager(
            puchittoSystemsProvider.ClientManager,
            puchittoSystemsProvider.MakeLogger<EntityManager>());

        _threadExecutorChannel = Channel.CreateUnbounded<RealmThreadActionCallback>();
        
        ParseEntities(levelDefinition);
        
        var maxId = EntityManager.Entities.Max(e => e.Id);
        IdAllocator = new EntityIdAllocator(maxId + 1);
    }

    /// <summary>
    /// Dispatches an action on the realm's ticking thread.
    /// </summary>
    /// <param name="action">
    /// The action to run.
    /// </param>
    public async Task DispatchOnRealmThread(RealmThreadActionCallback action)
    {
        await _threadExecutorChannel.Writer.WriteAsync(action);
    }

    /// <summary>
    /// Ticks this realm.
    /// </summary>
    public async Task Tick()
    {
        // Drain the thread executor channel.
        while (_threadExecutorChannel.Reader.TryRead(out var action))
        {
            await action(this);
        }
        
        
    }

    /// <summary>
    /// Begins admitting a client into this realm.
    /// </summary>
    /// <param name="client">The client.</param>
    public async Task BeginClientAdmit(Client client)
    {
        client.CurrentRealm = this;
        
        var downloadPath = _definition.RemotePackagePath ?? _definition.LocalPackagePath;
        client.SetState(ClientState.Connecting);
        
        await client.SendData(new LoadPacket
        {
            LevelName = downloadPath
        });
    }

    /// <summary>
    /// Parses the entities within this realm.
    /// </summary>
    private void ParseEntities(Level levelDefinition)
    {
        foreach (var entityDefinition in levelDefinition.Entities)
        {
            var ent = SystemsProvider.EntityFactory
                .CreateEntityFromLevelData(this, entityDefinition);
            
            EntityManager.AddEntity(ent);
        }
    }
    
    /// <summary>
    /// Spawns a player given the rules.
    /// </summary>
    /// <param name="client">The connecting client.</param>
    /// <param name="rules">The rules.</param>
    public async Task SpawnPlayer(Client client, IGameServerRules rules)
    {
        client.CurrentRealm = this;

        await EntityManager.SpawnMissingEntitiesFor(client);

        var entity = rules.CreateEntityForClient(this, client);
        entity.Owner = client;
        
        await EntityManager.AddAndSpawnForEveryone(entity);

        if (OnClientJoinedRealm is not null)
        {
            await OnClientJoinedRealm.Invoke(client);
        }
    }
}