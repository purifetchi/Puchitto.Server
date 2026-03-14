namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// A string atom.
/// </summary>
public class Text : IMiniAnticsAtom
{
    /// <summary>
    /// The value of the text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructs a new text atom.
    /// </summary>
    /// <param name="value">
    /// The value of the text.
    /// </param>
    public Text(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public string ToAbstractRepresentation()
    {
        return $"Text[{Value}]";
    }

    /// <inheritdoc />
    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return Value;
    }
}