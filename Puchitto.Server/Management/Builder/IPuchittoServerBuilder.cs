using Microsoft.Extensions.Logging;

namespace Puchitto.Server.Management.Builder;

/// <summary>
/// The final server builder step.
/// </summary>
public interface IPuchittoServerBuilder
{
    /// <summary>
    /// Configures the server options.
    /// </summary>
    /// <param name="configureAction">The configure action.</param>
    /// <returns>The server builder.</returns>
    IPuchittoServerBuilder Configure(Action<PuchittoServerConfig> configureAction);
    
    /// <summary>
    /// Configures the logging for this server.
    /// </summary>
    /// <param name="configureLogging">The logging.</param>
    /// <returns>The server builder.</returns>
    IPuchittoServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging);
    
    /// <summary>
    /// Builds the server.
    /// </summary>
    /// <returns>The built server.</returns>
    PuchittoServer Build();
}