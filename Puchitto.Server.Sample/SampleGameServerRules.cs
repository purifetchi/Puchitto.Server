using Puchitto.Server.Clients;
using Puchitto.Server.Game;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Packets;
using Puchitto.Server.Realms;
using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Sample;

public class SampleGameServerRules : AbstractGameServerRules
{
    public override string Name => "DummyGame";

    public override void OnReady()
    {
        RealmRegistry.AddRealm("flatland", new RealmDefinition("flatland", "flatland.alf", Flags: RealmFlags.Default));
    }

    public override void RegisterPackets(PacketRegistry registry)
    {
        registry.RegisterHandler<RequestWalkPacket>(OnRequestWalk);
    }

    private async Task OnRequestWalk(RequestWalkPacket packet, Client client)
    {
        // Find the entity for this player.
        var entity = PuchittoSystemsProvider
            .RealmManager
            .Default
            .EntityManager
            .Entities
            .FirstOrDefault(ent => ent.Owner?.Id == client.Id && ent is UnknownEntity);

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

    public override BaseEntity CreateEntityForClient(Realm realm, Client client)
    {
        return new UnknownEntity
        {
            Id = PuchittoSystemsProvider.RealmManager.Default.IdAllocator.GetNextId()
        };
    }
}