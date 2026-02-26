using Microsoft.Extensions.Logging;

namespace Puchitto.Server.Management;

public class PuchittoServerConfig
{
    /// <summary>
    /// The list of prefixes the Puchitto server will be listening on.
    /// </summary>
    public required string[] Prefixes { get; set; }
    
    /// <summary>
    /// The logger configuration.
    /// </summary>
    public Action<ILoggingBuilder>? LoggingBuilder { get; set; }
    
    // TODO: SSL
}