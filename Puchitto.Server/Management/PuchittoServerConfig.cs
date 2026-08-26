namespace Puchitto.Server.Management;

/// <summary>
/// The configuration for a puchitto server.
/// </summary>
public class PuchittoServerConfig
{
    /// <summary>
    /// How often do realms tick?
    /// </summary>
    public int RealmTicksPerSecond { get; set; } = 20;
    
    /// <summary>
    /// The amount of people one realm can fit. Null means infinite.
    /// </summary>
    public int? RealmOccupancyLimit { get; set; }
}