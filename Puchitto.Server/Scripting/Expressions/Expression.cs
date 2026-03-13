namespace Puchitto.Server.Scripting.Expressions;

public class Expression : IMiniAnticsAtom
{
    public List<IMiniAnticsAtom> Atoms { get; }

    public Expression(List<IMiniAnticsAtom> atoms)
    {
        Atoms = atoms;
    }

    public string ToAbstractRepresentation()
    {
        var values = Atoms.Select(a => a.ToAbstractRepresentation());
        var joined = string.Join(", ", values);

        return $"Expression[{joined}]";
    }

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