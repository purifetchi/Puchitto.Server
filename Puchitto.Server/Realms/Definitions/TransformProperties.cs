using System.Numerics;
using System.Text.Json.Serialization;
using Puchitto.Server.Realms.Definitions.Converters;

namespace Puchitto.Server.Realms.Definitions;

public class TransformProperties
{
    [JsonPropertyName("position"), JsonConverter(typeof(ArrayVectorConverter))]
    public Vector3 Position { get; set; }
        
    [JsonPropertyName("rotation"), JsonConverter(typeof(ArrayQuaternionConverter))]
    public Quaternion Rotation { get; set; }
        
    [JsonPropertyName("scale"), JsonConverter(typeof(ArrayVectorConverter))]
    public Vector3 Scale { get; set; }
}