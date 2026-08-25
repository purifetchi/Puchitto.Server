namespace Puchitto.Server.Realms;

/// <summary>
/// Defines a single realm link of format realm://name?query=value
/// </summary>
/// <param name="RealmName">The name of the realm.</param>
/// <param name="QueryPath">The query string.</param>
public record RealmLink(
    string RealmName,
    string QueryPath)
{
    /// <summary>
    /// Parses a link into a realm link.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns>The realm link if it was succesfully deserialized.</returns>
    public static RealmLink TryParse(string link)
    {
        var uri = new Uri(link);

        if (uri.Scheme != "realm")
        {
            // TODO: Better exception.
            throw new InvalidOperationException("Invalid realm link");
        }

        var realmName = uri.Host;
        var query = uri.Query.TrimStart('?');
        
        return new RealmLink(realmName, query);
    }

    /// <summary>
    /// Turns this realm link into a URI string.
    /// </summary>
    /// <returns>The URI string.</returns>
    public string ToUriString()
    {
        var query = !string.IsNullOrWhiteSpace(QueryPath)
            ? $"?{QueryPath}"
            : string.Empty;
        
        return $"realm://{RealmName}{query}";
    }
}