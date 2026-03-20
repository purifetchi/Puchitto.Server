using Puchitto.Server.Scripting;

namespace Puchitto.Server.Game.Entities.Scripting;

/// <summary>
/// A singular antics definition for an entity.
/// </summary>
public class ObjectAntics
{
    /// <summary>
    /// When should the antics run?
    /// </summary>
    public required AnticsOn On { get; set; }
    
    /// <summary>
    /// The script.
    /// </summary>
    public required MiniAnticsScript Script { get; set; }
    
    /// <summary>
    /// The name of the MiniAntics script.
    /// </summary>
    public string? Name { get; set; }
}