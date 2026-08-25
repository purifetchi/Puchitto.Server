using System.Buffers;
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
    /// A definition of a client message dispatched on the realm.
    /// </summary>
    private readonly record struct RealmClientMessage(int OpCode, Client Client, byte[] Payload);
    
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
    /// The client message channel for this realm.
    /// </summary>
    private readonly Channel<RealmClientMessage> _clientMessageChannel;
    
    /// <summary>
    /// Constructs a new Realm.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The systems' provider.
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
        _clientMessageChannel = Channel.CreateBounded<RealmClientMessage>(new BoundedChannelOptions(1024)
        {
            SingleReader = true,
            SingleWriter = false
        });
        
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
    public void DispatchOnRealmThread(RealmThreadActionCallback action)
    {
        _threadExecutorChannel.Writer.TryWrite(action);
    }

    /// <summary>
    /// Gets an entity by its id on this realm.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <typeparam name="TEntity">The entity.</typeparam>
    /// <returns>The entity that has this id.</returns>
    public TEntity? GetEntityById<TEntity>(int id)
        where TEntity : BaseEntity
    {
        return EntityManager.GetEntityById<TEntity>(id);
    }

    /// <summary>
    /// Returns the first found entity for a given client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <typeparam name="TEntity">The entity,</typeparam>
    /// <returns>The entity that is associated with this player.</returns>
    public TEntity? PlayerEntityOf<TEntity>(Client client)
        where TEntity : BaseEntity
    {
        var entity = EntityManager.Entities;
        return entity.FirstOrDefault(e => e is TEntity && e.Owner == client) as TEntity;
    }

    /// <summary>
    /// Enqueues a client message for processing on the realm thread.
    /// </summary>
    /// <param name="opCode">The opcode of the message.</param>
    /// <param name="client">The client.</param>
    /// <param name="slice">The array segment containing the payload.</param>
    public async Task EnqueueClientMessage(
        int opCode,
        Client client,
        ArraySegment<byte> slice)
    {
        // Rent an array
        var array = ArrayPool<byte>.Shared.Rent(slice.Count);
        slice.CopyTo(array);
        
        await _clientMessageChannel.Writer.WriteAsync(new RealmClientMessage(opCode, client, array));
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

        // Handle all the incoming client messages.
        while (_clientMessageChannel.Reader.TryRead(out var message))
        {
            await SystemsProvider.Registry.ExecuteHandler(message.OpCode, message.Payload, message.Client);
            ArrayPool<byte>.Shared.Return(message.Payload);
        }
        
        // TODO: Tick entities.
    }

    /// <summary>
    /// Posts an admission into this realm for a client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="query">The query.</param>
    public void PostAdmit(Client client, string query)
    {
        // TODO: Make use of the query.
        DispatchOnRealmThread(async realm =>
        {
            await realm.BeginClientAdmit(client);
        });
    }

    /// <summary>
    /// Begins admitting a client into this realm.
    /// </summary>
    /// <param name="client">The client.</param>
    private async Task BeginClientAdmit(Client client)
    {
        client.CurrentRealm = this;
        
        var downloadPath = _definition.RemotePackagePath ?? _definition.LocalPackagePath;
        
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
    /// Removes a client from the realm.
    /// </summary>
    /// <param name="client">
    /// The client to remove.
    /// </param>
    public async Task RemoveClient(Client client)
    {
        // TODO: Are we sure players wont have more than one entity?
        var playerEntity = PlayerEntityOf<BaseEntity>(client);
        if (playerEntity is not null)
        {
            await EntityManager.RemoveAndDespawn(playerEntity);
        }
        
        if (OnClientLeftRealm is not null)
        {
            await OnClientLeftRealm.Invoke(client);
        }
    }
    
    /// <summary>
    /// Spawns a player given the rules.
    /// </summary>
    /// <param name="client">The connecting client.</param>
    /// <param name="rules">The rules.</param>
    public async Task SpawnPlayer(Client client, IGameServerRules rules)
    {
        client.SetState(ClientState.Present);
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