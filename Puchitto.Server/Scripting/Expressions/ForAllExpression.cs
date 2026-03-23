using System.Collections;

namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// The for-all expression.
/// </summary>
public class ForAllExpression : IMiniAnticsAtom
{
    public string VariableName { get; }
    
    public IMiniAnticsAtom Collection { get; }
    
    public IMiniAnticsAtom Body { get; }
    
    public ForAllExpression(List<IMiniAnticsAtom> atoms)
    {
        if (atoms.Count != 3)
        {
            throw new MiniAnticsParseException($"for-all expression expects 3 atoms, found {atoms.Count}");
        }

        if (atoms[0] is not Symbol variableNameSymbol)
        {
            throw new MiniAnticsParseException(
                $"for-all expression expects the first argument to be a Symbol, found {atoms[0].GetType().Name}");
        }
        
        VariableName = variableNameSymbol.Name;
        Collection = atoms[1];
        Body = atoms[2];
    }
    
    public string ToAbstractRepresentation()
    {
        return $"ForAll<{VariableName} in {Collection.ToAbstractRepresentation()}>[{Body.ToAbstractRepresentation()}]";
    }

    public object? Evaluate(MiniAnticsEnvironment env)
    {
        // Try to resolve either an IEnumerator or an IEnumerable.
        var resolved = Collection.Evaluate(env);
        var maybeEnumerator = TryResolveFromObject(resolved);

        if (maybeEnumerator == null &&
            resolved is Delegate @delegate)
        {
            var delegateResult =  @delegate.DynamicInvoke();
            maybeEnumerator = TryResolveFromObject(delegateResult);
        }

        if (maybeEnumerator == null)
        {
            throw new MiniAnticsRuntimeException("The for-all collection did not result in an IEnumerator.");
        }
        
        IterateEnumerator(env, maybeEnumerator);
        
        return null;
    }

    /// <summary>
    /// Tries to resolve an IEnumerator from an object.
    /// </summary>
    /// <param name="obj">
    /// The object.
    /// </param>
    /// <returns>
    /// The enumerator.
    /// </returns>
    private IEnumerator? TryResolveFromObject(object? obj)
    {
        if (obj is IEnumerator maybeEnumerator)
        {
            return maybeEnumerator;
        }

        if (obj is IEnumerable enumerable)
        {
            return enumerable.GetEnumerator();
        }

        return null;
    }

    private void IterateEnumerator(
        MiniAnticsEnvironment env,
        IEnumerator enumerator)
    {
        // Create a new environment.
        var childEnv = new MiniAnticsEnvironment(env);
        
        // And now, enumerate and run the body for as long as we need.
        while (enumerator.MoveNext())
        {
            childEnv.Set(VariableName, enumerator.Current!);
            Body.Evaluate(childEnv);
        }
    }
}