using Puchitto.Server.Clients;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// The base class for an entity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// The type of the entity.
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    /// The ID of the entity.
    /// </summary>
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// Is the current entity authored by the level?
    /// </summary>
    public bool IsAuthored { get; set; }

    /// <summary>
    /// The transform of the entity.
    /// </summary>
    public Transform Transform { get; } = new();

    /// <summary>
    /// The owning client.
    /// </summary>
    public Client? Owner { get; set; }

    /// <summary>
    /// Gets the entity data for serialization.
    /// </summary>
    /// <returns>
    /// The entity data.
    /// </returns>
    public object GetEntityDataForSerialization()
    {
        // TODO
        return new object();
    }
}