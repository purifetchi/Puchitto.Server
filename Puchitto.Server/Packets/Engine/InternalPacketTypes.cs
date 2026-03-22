namespace Puchitto.Server.Packets.Engine;

/// <summary>
/// The internal packet types.
/// </summary>
public enum InternalPacketTypes
{
    /// <summary>
    /// Server -> Client: Branding and game rules information
    /// </summary>
    Hello = 1,
    
    /// <summary>
    /// Client -> Server: Join realm
    /// </summary>
    Join = 2,
    
    /// <summary>
    /// Server -> Client: Load specific realm file
    /// </summary>
    Load = 3,
    
    /// <summary>
    /// Client -> Server: Update the current load state
    /// </summary>
    LoadStateUpdate = 4,
    
    /// <summary>
    /// Server -> Client: Create given entity
    /// </summary>
    CreateEntity = 5,
    
    /// <summary>
    /// Server -> Client: Removes a given entity
    /// </summary>
    RemoveEntity = 6,
    
    /// <summary>
    /// Server &lt;-&gt; Client: A MiniAntics RPC call
    /// </summary>
    MiniAnticsRpc = 7,
    
    /// <summary>
    /// Server &lt;-&gt; Client: A heartbeat.
    /// </summary>
    KeepAlive = 8
}