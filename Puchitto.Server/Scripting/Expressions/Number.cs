namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// A floating-point number.
/// </summary>
public class Number : IMiniAnticsAtom
{
    /// <summary>
    /// The value of the number.
    /// </summary>
    public float Value { get; }

    /// <summary>
    /// Constructs a new number atom.
    /// </summary>
    /// <param name="value">
    /// The value of the number.
    /// </param>
    public Number(float value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public string ToAbstractRepresentation()
    {
        return $"Number[{Value}]";
    }

    /// <inheritdoc />
    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return Value;
    }
}