using Puchitto.Server.Game;

namespace Puchitto.Server.Management.Builder;

/// <summary>
/// The initial step for configuring the rules being used.
/// </summary>
public interface IRulesBuilderStep
{
    /// <summary>
    /// Tells the server to use a given set of rules.
    /// </summary>
    /// <param name="rules">The rules to use.</param>
    /// <returns>The endpoint builder step.</returns>
    IEndpointBuilderStep UseRules(IGameServerRules rules);
    
    /// <summary>
    /// Tells the server to create a new rules set of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <returns>The endpoint builder step.</returns>
    IEndpointBuilderStep UseRules<T>() where T : IGameServerRules, new();
}