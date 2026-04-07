using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puchitto.Server.Game.Entities.Serialization.Converters;

/// <summary>
/// A factory for EntityReference converters.
/// </summary>
public class EntityReferenceConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && 
               typeToConvert.GetGenericTypeDefinition() == typeof(EntityReference<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityType = typeToConvert.GetGenericArguments()[0];
        
        return (JsonConverter)Activator.CreateInstance(
            typeof(EntityReferenceConverter<>).MakeGenericType(entityType))!;
    }
}

/// <summary>
/// Deserializes EntityReferences from JSON.
/// </summary>
public class EntityReferenceConverter<TEntity> : JsonConverter<EntityReference<TEntity>>
    where TEntity : BaseEntity
{
    /// <inheritdoc />
    public override EntityReference<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for EntityReference, but got {reader.TokenType}");
        }
        
        return new EntityReference<TEntity>
        {
            Name = reader.GetString() ?? string.Empty
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EntityReference<TEntity> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}