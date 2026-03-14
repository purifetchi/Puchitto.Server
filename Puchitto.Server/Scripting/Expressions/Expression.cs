namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// An expression consisting of other atoms.
/// </summary>
public class Expression : IMiniAnticsAtom
{
    /// <summary>
    /// The list of atoms taking part in this expression.
    /// </summary>
    public List<IMiniAnticsAtom> Atoms { get; }

    /// <summary>
    /// Constructs a new expression.
    /// </summary>
    /// <param name="atoms">
    /// The list of atoms taking part in this expression.
    /// </param>
    public Expression(List<IMiniAnticsAtom> atoms)
    {
        Atoms = atoms;
    }

    /// <inheritdoc />
    public string ToAbstractRepresentation()
    {
        var values = Atoms.Select(a => a.ToAbstractRepresentation());
        var joined = string.Join(", ", values);

        return $"Expression[{joined}]";
    }

    /// <inheritdoc />
    public object? Evaluate(MiniAnticsEnvironment env)
    {
        var head = Atoms[0];
        var evaluated = head.Evaluate(env);
        
        if (evaluated is not Delegate invokable)
        {
            throw new InvalidOperationException("Tried to execute a MiniAntics object that's not a function call.");
        }

        var values = Atoms[1..].Select(a => a.Evaluate(env)).ToArray();
        return invokable.DynamicInvoke(values);
    }
}