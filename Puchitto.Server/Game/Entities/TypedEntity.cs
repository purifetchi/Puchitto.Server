using System.Text.Json;
using Puchitto.Server.Realms.Definitions.Converters;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// An entity with its own data.
/// </summary>
/// <typeparam name="TDataType">
/// The data for this entity.
/// </typeparam>
public abstract class TypedEntity<TDataType> : BaseEntity
{
    /// <summary>
    /// The default deserializer.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new ArrayQuaternionConverter(),
            new ArrayVectorConverter()
        }
    };
    
    /// <summary>
    /// The data for this entity.
    /// </summary>
    public TDataType Data { get; set; } = default!;

    /// <inheritdoc />
    public override void LoadEntityData(JsonElement data)
    {
        Data = data.Deserialize<TDataType>(Options)!;
    }

    /// <inheritdoc />
    public override object GetEntityDataForSerialization()
    {
        return Data;
    }
}