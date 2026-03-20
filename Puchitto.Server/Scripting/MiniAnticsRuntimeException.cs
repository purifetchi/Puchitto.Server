namespace Puchitto.Server.Scripting;

/// <summary>
/// Thrown when we encounter a MiniAntics runtime exception.
/// </summary>
/// <param name="reason">The reason it failed to run.</param>
public class MiniAnticsRuntimeException(string reason)
    : Exception($"Failed to run MiniAntics script. {reason}")
{
    
}