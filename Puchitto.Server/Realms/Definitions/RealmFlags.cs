namespace Puchitto.Server.Realms.Definitions;

/// <summary>
/// Flags describing a realm.
/// </summary>
[Flags]
public enum RealmFlags
{
    None = 0,
    Persistent = 1 << 0,
    Default = Persistent | 1 << 1
}