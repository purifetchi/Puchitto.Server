using Puchitto.Server.Clients;
using Puchitto.Server.Game.Entities;
using Puchitto.Server.Management;
using Puchitto.Server.Packets;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Game;

public abstract class AbstractGameServerRules : IGameServerRules
{
    public abstract string Name { get; }

    public IPuchittoSystemsProvider PuchittoSystemsProvider { get; set; } = null!;

    public IRealmRegistry RealmRegistry { get; set; } = new RealmRegistry();

    public virtual void Attach(PuchittoServer server)
    {
        PuchittoSystemsProvider = server;
    }

    public virtual void OnReady()
    {
        
    }

    public virtual void RegisterPackets(PacketRegistry registry)
    {
        
    }

    public virtual void RegisterEntities(EntityFactory entityFactory)
    {
    }

    public abstract void ConfigureRealms(IRealmRegistry realmRegistry);

    public abstract BaseEntity CreateEntityForClient(Realm realm, Client client);
}