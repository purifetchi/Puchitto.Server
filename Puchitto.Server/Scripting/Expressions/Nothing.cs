namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// An atom representing quite literally nothing.
/// </summary>
public class Nothing : IMiniAnticsAtom
{
    /// <summary>
    /// A singleton implementation of nothing.
    /// </summary>
    public static Nothing Default { get; } = new();
    
    /// <inheritdoc />
    public string ToAbstractRepresentation() => "Nothing";

    /// <inheritdoc />
    public object? Evaluate(MiniAnticsEnvironment env) => null;
}