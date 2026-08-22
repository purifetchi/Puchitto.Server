using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

public interface IRealmRegistry
{
    IEnumerable<RealmDefinition> GetRealmDefinitions();
    
    void AddRealm(string name, RealmDefinition definition);
    
    RealmDefinition? GetDefinitionForRealm(string name);
}