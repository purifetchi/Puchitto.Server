namespace Puchitto.Server.Management.Builder;

/// <summary>
/// Thrown by the builder when we can't build the server.
/// </summary>
/// <param name="reason">The reason.</param>
public class PuchittoBuilderException(string reason)
    : Exception($"Failed to build the Puchitto server! {reason}");