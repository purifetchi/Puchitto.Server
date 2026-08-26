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

    public override void RegisterPackets(PacketRegistry registry)
    {
        registry.RegisterRealmHandler<RequestWalkPacket>(OnRequestWalk);
    }

    private async Task OnRequestWalk(RequestWalkPacket packet, Realm realm, Client client)
    {
        // Find the entity for this player.
        var entity = realm.PlayerEntityOf<BaseEntity>(client);
        if (entity == null)
        {
            return;
        }

        entity.Transform.Position = packet.To;
        
        var movePacket = new MoveAtaPacket
        {
            Id = entity.Id,
            To = packet.To
        };

        await realm.SendToClients(movePacket, excluding: client);
    }

    public override void ConfigureRealms(IRealmRegistry realmRegistry)
    {
        realmRegistry.AddRealm("flatland", new RealmDefinition("flatland", "flatland.alf", Flags: RealmFlags.Default));
    }

    public override BaseEntity CreateEntityForClient(Realm realm, Client client)
    {
        return realm.CreateEntity<UnknownEntity>(owner: client);
    }
}