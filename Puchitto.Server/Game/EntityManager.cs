using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Puchitto.Server.Clients;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Packets.Engine.Clientbound;

namespace Puchitto.Server.Game;

public class EntityManager
{
    /// <summary>
    /// The concurrent list holding all the entities.
    /// </summary>
    private readonly List<BaseEntity> _entities = new();

    /// <summary>
    /// The client manager.
    /// </summary>
    private readonly ClientManager _clientManager;
    
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<EntityManager> _logger;
    
    /// <summary>
    /// The entity lock.
    /// </summary>
    private readonly Lock _entityLock = new();
    
    /// <summary>
    /// The list of entities.
    /// </summary>
    public IReadOnlyList<BaseEntity> Entities => _entities;
    
    /// <summary>
    /// Constructs a new entity manager.
    /// </summary>
    public EntityManager(
        ClientManager clientManager,
        ILogger<EntityManager> logger)
    {
        _logger = logger;
        _clientManager = clientManager;
    }

    /// <summary>
    /// Adds an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public void AddEntity(BaseEntity entity)
    {
        lock (_entityLock)
        {
            _entities.Add(entity);
        }
    }

    /// <summary>
    /// Adds an entity and spawns it for all clients.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public async Task AddAndSpawnForEveryone(BaseEntity entity)
    {
        AddEntity(entity);

        var entityData = entity.GetEntityDataForSerialization();
        var serializedEntityData = JsonSerializer.Serialize(entityData);
        
        foreach (var client in _clientManager.Clients)
        {
            var isOwner = entity.Owner?.Id == client.Id;
            var packet = new CreateEntityPacket
            {
                Id = entity.Id,
                EntityName = entity.Type,
                Position = entity.Transform.Position,
                Rotation = entity.Transform.Rotation,
                Scale = entity.Transform.Scale,
                IsOwner = isOwner,
                JsonEntityData = serializedEntityData
            };

            await client.SendData(packet);
        }
    }

    /// <summary>
    /// Spawns the missing entities for a given client.
    /// </summary>
    /// <param name="client">
    /// The client.
    /// </param>
    public async Task SpawnMissingEntitiesFor(Client client)
    {
        var packetList = new List<CreateEntityPacket>();
        lock (_entityLock)
        {
            foreach (var entity in _entities)
            {
                var entityData = entity.GetEntityDataForSerialization();
                var serializedEntityData = JsonSerializer.Serialize(entityData);
                var isOwner = entity.Owner?.Id == client.Id;
                packetList.Add(new CreateEntityPacket
                {
                    Id = entity.Id,
                    EntityName = entity.Type,
                    Position = entity.Transform.Position,
                    Rotation = entity.Transform.Rotation,
                    Scale = entity.Transform.Scale,
                    IsOwner = isOwner,
                    JsonEntityData = serializedEntityData
                });
            }
        }

        // NOTE: We need to do it this way, as we cannot await inside of locks.
        //       Maybe we should move to semaphores?
        foreach (var packet in packetList)
        {
            await client.SendData(packet);
        }
    }

    /// <summary>
    /// Removes the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public void RemoveEntity(BaseEntity entity)
    {
        lock (_entityLock)
        {
            _entities.Remove(entity);
        }
    }
}