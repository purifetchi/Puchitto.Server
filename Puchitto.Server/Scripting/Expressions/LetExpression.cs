namespace Puchitto.Server.Scripting.Expressions;

public class LetExpression : IMiniAnticsAtom
{
    private class Assignment
    {
        public required Symbol Name { get; set; }
        
        public required IMiniAnticsAtom Value { get; set; }
    }
    
    public IMiniAnticsAtom Method { get; }

    private readonly List<Assignment> _varlist;
    
    public LetExpression(List<IMiniAnticsAtom> atoms)
    {
        if (atoms.Count != 2)
        {
            throw new MiniAnticsParseException($"let expression expects 2 atoms, found {atoms.Count}");
        }

        _varlist = GetVarlist(atoms[0]);
        Method = atoms[1];
    }

    private List<Assignment> GetVarlist(IMiniAnticsAtom atom)
    {
        if (atom is not Expression varlistExpression)
        {
            throw new MiniAnticsParseException("First parameter of let must be an expression.");
        }

        var assignments = new List<Assignment>();
        foreach (var assignment in varlistExpression.Atoms)
        {
            if (assignment is not Expression assignmentExpression)
            {
                throw new MiniAnticsParseException("The varlist must consist of expressions.");
            }

            if (assignmentExpression.Atoms.Count != 2)
            {
                throw new MiniAnticsParseException($"The assignment expression must contain two atoms. Found {assignmentExpression.Atoms.Count}");
            }

            if (assignmentExpression.Atoms[0] is not Symbol assignmentSymbol)
            {
                throw new MiniAnticsParseException(
                    $"The first atom of the assignment expression was not a symbol. Found {assignmentExpression.Atoms[0].GetType().Name} instead.");
            }
            
            assignments.Add(new Assignment
            {
                Name = assignmentSymbol,
                Value = assignmentExpression.Atoms[1]
            });
        }
        
        return assignments;
    }
    
    public string ToAbstractRepresentation()
    {
        var varlist = _varlist.Select(a => $"{a.Name.Name}={a.Value.ToAbstractRepresentation()}");
        var joined = string.Join(", ", varlist);
        return $"Let<{joined}>[{Method.ToAbstractRepresentation()}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        // Create a new temporary environment parented to the previous environment.
        var newEnv = new MiniAnticsEnvironment(env);
        
        // Assign all the values
        foreach (var assignment in _varlist)
        {
            newEnv.Set(assignment.Name.Name, assignment.Value.Evaluate(env)!);
        }
        
        // Evaluate the method within the confines of the new environment.
        return Method.Evaluate(newEnv);
    }
}