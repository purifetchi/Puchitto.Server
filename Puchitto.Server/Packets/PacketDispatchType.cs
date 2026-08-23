namespace Puchitto.Server.Packets;

/// <summary>
/// The type where the packet will be dispatched.
/// </summary>
public enum PacketDispatchType
{
    /// <summary>
    /// Global server packets.
    /// </summary>
    Server,
    
    /// <summary>
    /// Per-realm packets.
    /// </summary>
    Realm
}