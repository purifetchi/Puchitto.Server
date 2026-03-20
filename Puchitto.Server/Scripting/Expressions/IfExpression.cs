namespace Puchitto.Server.Scripting.Expressions;

public class IfExpression : IMiniAnticsAtom
{
    public IMiniAnticsAtom Condition { get; }
    
    public IMiniAnticsAtom Then { get; }
    
    public IMiniAnticsAtom Else { get; }

    public IfExpression(List<IMiniAnticsAtom> atoms)
    {
        if (atoms.Count != 3)
        {
            throw new MiniAnticsParseException($"If expression expected 3 atoms, got only {atoms.Count}.");
        }
        
        Condition = atoms[0];
        Then = atoms[1];
        Else = atoms[2];
    }
    
    public string ToAbstractRepresentation()
    {
        return
            $"If<{Condition.ToAbstractRepresentation()}>[{Then.ToAbstractRepresentation()}][{Else.ToAbstractRepresentation()}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        var condition = Condition.Evaluate(env);
        return condition is true
            ? Then.Evaluate(env)
            : Else.Evaluate(env);
    }
}