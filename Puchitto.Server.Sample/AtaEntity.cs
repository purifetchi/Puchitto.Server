using Puchitto.Server.Game.Entities;
using Puchitto.Server.Game.Entities.Scripting;
using Puchitto.Server.Management;
using Puchitto.Server.Scripting;

namespace Puchitto.Server.Sample;

public class AtaEntity : BaseEntity
{
    public override string Type => "ata";

    public AtaEntity(IPuchittoSystemsProvider puchittoSystemsProvider)
        : base(puchittoSystemsProvider)
    {
        
    }
}