namespace Puchitto.Server.Scripting.Expressions;

public class Symbol : IMiniAnticsAtom
{
    public string Name { get; }

    public Symbol(string name)
    {
        Name = name;
    }

    public string ToAbstractRepresentation()
    {
        return $"Symbol[{Name}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return env.Get(Name) ?? throw new Exception("Symbol wasn't bound to anything.");
    }
}