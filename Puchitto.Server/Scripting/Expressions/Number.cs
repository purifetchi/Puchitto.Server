namespace Puchitto.Server.Scripting.Expressions;

public class Number : IMiniAnticsAtom
{
    public float Value { get; }

    public Number(float value)
    {
        Value = value;
    }

    public string ToAbstractRepresentation()
    {
        return $"Number[{Value}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return Value;
    }
}