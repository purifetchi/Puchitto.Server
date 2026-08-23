using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

/// <summary>
/// A singular realm slot.
/// </summary>
/// <param name="Realm">The realm attached to this slot.</param>
/// <param name="CancellationTokenSource">The cancellation token source for the realm.</param>
public record RealmSlot(
    RealmState State,
    Realm Realm,
    CancellationTokenSource CancellationTokenSource);