namespace Puchitto.Server.Scripting.Expressions;

public class PrognExpression : IMiniAnticsAtom
{
    public List<IMiniAnticsAtom> Atoms { get; }

    public PrognExpression(List<IMiniAnticsAtom> atoms)
    {
        Atoms = atoms;
    }

    public string ToAbstractRepresentation()
    {
        var values = Atoms.Select(a => a.ToAbstractRepresentation());
        var joined = string.Join(", ", values);

        return $"Progn[{joined}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        object? last = null;
        foreach (var miniAnticsAtom in Atoms)
        {
            last = miniAnticsAtom.Evaluate(env);
        }

        return last;
    }
}