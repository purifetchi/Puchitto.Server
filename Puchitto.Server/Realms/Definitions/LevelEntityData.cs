using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puchitto.Server.Realms.Definitions;

public class LevelEntityData
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    [JsonPropertyName("transform")]
    public required TransformProperties Transform { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
        
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }
    
    [JsonPropertyName("visible")]
    public bool Visible { get; set; }
        
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
        
    [JsonPropertyName("antics")]
    public AnticsDefinition[]? Antics { get; set; }
}