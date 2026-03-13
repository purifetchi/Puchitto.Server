namespace Puchitto.Server.Scripting.Expressions;

public interface IMiniAnticsAtom
{
    string ToAbstractRepresentation();
    object? Evaluate(MiniAnticsEnvironment env);
}