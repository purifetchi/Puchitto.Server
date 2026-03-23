namespace Puchitto.Server.Scripting;

public class MiniAnticsEnvironment
{
    private Dictionary<string, object> _valueMap;
    private MiniAnticsEnvironment? _parent;

    public MiniAnticsEnvironment(MiniAnticsEnvironment? parent = null)
    {
        _valueMap = new Dictionary<string, object>();
        _parent = parent;
    }

    public void Set(string key, object value)
    {
        _valueMap[key] = value;
    }

    public void Unset(string key)
    {
        _valueMap.Remove(key);
    }
    
    public object? Get(string key)
    {
        if (_valueMap.TryGetValue(key, out var value))
        {
            return value;
        }

        return _parent?.Get(key);
    }
}