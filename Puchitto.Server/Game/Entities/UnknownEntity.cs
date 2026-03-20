using Puchitto.Server.Management;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// An entity whose type we do not know, but we still might be interested in the antics it has.
/// </summary>
public class UnknownEntity : BaseEntity
{
    public UnknownEntity(LevelEntityData entityData,
        IPuchittoSystemsProvider puchittoSystemsProvider)
        : base(puchittoSystemsProvider, entityData)
    {
        Name = entityData.Name;
    }

    public override string Type { get; } = "unknown";
}