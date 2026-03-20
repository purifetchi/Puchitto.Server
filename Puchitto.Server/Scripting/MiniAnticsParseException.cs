namespace Puchitto.Server.Scripting;

/// <summary>
/// Thrown when we encounter a MiniAntics parse exception.
/// </summary>
/// <param name="reason">The reason it failed to parse.</param>
public class MiniAnticsParseException( string reason)
    : Exception($"Failed to parse MiniAntics script. {reason}")
{
    
}