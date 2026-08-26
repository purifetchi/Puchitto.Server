namespace Puchitto.Server.Management.Builder;

/// <summary>
/// The endpoint builder step.
/// </summary>
public interface IEndpointBuilderStep
{
    /// <summary>
    /// Defines the URLs to listen on.
    /// </summary>
    /// <param name="urls">The URLs.</param>
    /// <returns>The final server builder.</returns>
    IPuchittoServerBuilder Listen(params string[] urls);
}