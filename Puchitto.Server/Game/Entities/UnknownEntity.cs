using Puchitto.Server.Game.Entities.Serialization;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// An entity whose type we do not know, but we still might be interested in the antics it has.
/// </summary>
public class UnknownEntity : TypedEntity<BlankEntityData>
{
    /// <inheritdoc />
    public override string Type { get; } = "unknown";
}