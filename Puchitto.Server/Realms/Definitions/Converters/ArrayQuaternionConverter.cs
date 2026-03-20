using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puchitto.Server.Realms.Definitions.Converters;

public class ArrayQuaternionConverter : JsonConverter<Quaternion>
{
    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteNumberValue(value.W);
        writer.WriteEndArray();
    }

    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token for Quaternion.");
        }

        var x = ReadNextSingle(ref reader);
        var y = ReadNextSingle(ref reader);
        var z = ReadNextSingle(ref reader);
        var w = ReadNextSingle(ref reader);

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndArray token after four float values.");
        }

        return new Quaternion(x, y, z, w);
    }

    private static float ReadNextSingle(ref Utf8JsonReader reader)
    {
        return !reader.Read()
            ? throw new JsonException("Unexpected end of JSON while reading Quaternion.")
            : reader.GetSingle();
    }
}