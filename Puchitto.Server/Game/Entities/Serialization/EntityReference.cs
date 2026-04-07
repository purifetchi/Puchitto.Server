using System.Text.Json.Serialization;
using Puchitto.Server.Game.Entities.Serialization.Converters;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Game.Entities.Serialization;

/// <summary>
/// Holds a reference to another entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
[JsonConverter(typeof(EntityReferenceConverterFactory))]
public struct EntityReference<TEntity>
    where TEntity: BaseEntity
{
    /// <summary>
    /// The name of the entity it's pointing towards
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Resolves the entity reference for a given realm.
    /// </summary>
    /// <param name="realm">The realm.</param>
    /// <returns>The entity pointed to by the reference.</returns>
    public TEntity? Resolve(Realm realm)
    {
        return realm.EntityManager
            .GetEntitiesByName<TEntity>(Name)
            .FirstOrDefault();
    }
}