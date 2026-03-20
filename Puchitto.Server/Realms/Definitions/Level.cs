using System.Text.Json.Serialization;

namespace Puchitto.Server.Realms.Definitions;

/// <summary>
/// The level.json file.
/// </summary>
public class Level
{
    [JsonPropertyName("ents")]
    public required IEnumerable<LevelEntityData> Entities { get; set; }
}