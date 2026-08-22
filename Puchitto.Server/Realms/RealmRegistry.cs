using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

public class RealmRegistry : IRealmRegistry
{
    private readonly Dictionary<string, RealmDefinition> _realms = new();
    
    public IEnumerable<RealmDefinition> GetRealmDefinitions()
    {
        return _realms.Values;
    }

    public void AddRealm(string name, RealmDefinition definition)
    {
        _realms.Add(name, definition);
    }

    public RealmDefinition? GetDefinitionForRealm(string name)
    {
        return _realms.GetValueOrDefault(name);
    }
}