using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puchitto.Server.Realms.Definitions.Converters;

public class ArrayVectorConverter : JsonConverter<Vector3>
{
    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }

    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token for Vector3.");
        }

        var x = ReadNextSingle(ref reader);
        var y = ReadNextSingle(ref reader);
        var z = ReadNextSingle(ref reader);

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndArray token after three float values.");
        }

        return new Vector3(x, y, z);
    }
    
    private static float ReadNextSingle(ref Utf8JsonReader reader)
    {
        return !reader.Read()
            ? throw new JsonException("Unexpected end of JSON while reading Vector3.")
            : reader.GetSingle();
    }
}