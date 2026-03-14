namespace Puchitto.Server.Game.Entities.Scripting;

/// <summary>
/// Defined when a MiniAntics script for an entity should run.
/// </summary>
public enum AnticsOn
{
    /// <summary>
    /// The script is ran when the object is attached (in the case of the server, when the object is created).
    /// </summary>
    Attach,
    
    /// <summary>
    /// The script is ran when the object is clicked (does nothing on the server).
    /// </summary>
    Click,
    
    /// <summary>
    /// The script is invoked by an RPC call.
    /// </summary>
    Rpc
}