using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets;

namespace Puchitto.Server.Sample;

public class SampleGameServerRules : IGameServerRules
{
    public string Name => "DummyGame";

    public IPuchittoSystemsProvider PuchittoSystemsProvider { get; set; } = null!;

    public void RegisterPackets(PacketRegistry registry)
    {
        registry.RegisterHandler<RequestWalkPacket>(OnRequestWalk);
    }

    private async Task OnRequestWalk(RequestWalkPacket packet, Client client)
    {
        // Find the entity for this player.
        var entity = PuchittoSystemsProvider
            .EntityManager
            .Entities
            .FirstOrDefault(ent => ent.Owner?.Id == client.Id && ent is AtaEntity);

        if (entity == null)
        {
            // how
            return;
        }

        entity.Transform.Position = packet.To;
        
        var movePacket = new MoveAtaPacket
        {
            Id = entity.Id,
            To = packet.To
        };
        
        foreach (var targetClient in PuchittoSystemsProvider.ClientManager.Clients)
        {
            if (targetClient == client)
            {
                continue;
            }

            await targetClient.SendData(movePacket);
        }
    }

    public string GetPackagePath()
    {
        return "/game/cooked.alf";
    }

    public BaseEntity CreateEntityForClient()
    {
        return new AtaEntity
        {
            Id = Guid.NewGuid().ToString()
        };
    }
}