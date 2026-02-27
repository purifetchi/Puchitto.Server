namespace Puchitto.Server.Clients;

/// <summary>
/// The current state of the client.
/// </summary>
public enum ClientState
{
    /// <summary>
    /// We've established a connection.
    /// </summary>
    Established,
    
    /// <summary>
    /// The client is connecting.
    /// </summary>
    Connecting,
    
    /// <summary>
    /// The client is loading.
    /// </summary>
    Loading,
    
    /// <summary>
    /// The client has loaded.
    /// </summary>
    Loaded,
    
    /// <summary>
    /// The client is present in the world.
    /// </summary>
    Present
}