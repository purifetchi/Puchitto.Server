namespace Puchitto.Server.Scripting.Expressions;

public class Text : IMiniAnticsAtom
{
    public string Value { get; }

    public Text(string value)
    {
        Value = value;
    }

    public string ToAbstractRepresentation()
    {
        return $"Text[{Value}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return Value;
    }
}