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
    /// Tries to parse a link into a realm link.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns>The realm link if it was succesfully deserialized.</returns>
    public static RealmLink? TryParse(string link)
    {
        var uri = new Uri(link);

        if (uri.Scheme != "realm")
        {
            return null;
        }

        var realmName = uri.Host;
        var query = uri.Query.TrimStart('?');
        
        return new RealmLink(realmName, query);
    }
}