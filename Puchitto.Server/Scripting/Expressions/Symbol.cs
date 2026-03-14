namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// A symbol that resolves to a value within the current environment.
/// </summary>
public class Symbol : IMiniAnticsAtom
{
    /// <summary>
    /// The name of the symbol.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Constructs a new symbol.
    /// </summary>
    /// <param name="name">
    /// The name of the symbol.
    /// </param>
    public Symbol(string name)
    {
        Name = name;
    }

    /// <inheritdoc />
    public string ToAbstractRepresentation()
    {
        return $"Symbol[{Name}]";
    }

    /// <inheritdoc />
    public object? Evaluate(MiniAnticsEnvironment env)
    {
        return env.Get(Name) ?? throw new Exception("Symbol wasn't bound to anything.");
    }
}