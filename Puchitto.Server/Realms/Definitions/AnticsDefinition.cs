using System.Text.Json.Serialization;

namespace Puchitto.Server.Realms.Definitions;

public class AnticsDefinition
{
    [JsonPropertyName("on")]
    public required string On { get; set; }

    [JsonPropertyName("script")]
    public required string Script { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}