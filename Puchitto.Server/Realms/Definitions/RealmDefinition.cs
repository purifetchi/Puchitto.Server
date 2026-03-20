namespace Puchitto.Server.Realms.Definitions;

/// <summary>
/// A definition of a single realm.
/// </summary>
/// <param name="IsDefault">Is this realm default?</param>
/// <param name="LocalPackagePath">The local path to the realm ALF package.</param>
/// <param name="RemotePackagePath">An optional remote path, if there's a place the client should fetch the package from.</param>
public record RealmDefinition(
    bool IsDefault,
    string LocalPackagePath,
    string? RemotePackagePath = null);