namespace Puchitto.Server.Scripting.Expressions;

/// <summary>
/// A MiniAntics atom, representing an object in an expression.
/// </summary>
public interface IMiniAnticsAtom
{
    /// <summary>
    /// Represents this atom in a textual form for debugging purposes. For example, a string will become `Text[...]`
    /// </summary>
    /// <returns>
    /// The textual form.
    /// </returns>
    string ToAbstractRepresentation();
    
    /// <summary>
    /// Evaluates this atom.
    /// </summary>
    /// <param name="env">
    /// The environment to evaluate under.
    /// </param>
    /// <returns>
    /// The value returned from this atom.
    /// </returns>
    object? Evaluate(MiniAnticsEnvironment env);
}