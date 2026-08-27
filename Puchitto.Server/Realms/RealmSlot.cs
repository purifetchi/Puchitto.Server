using Puchitto.Server.Realms.Definitions;

namespace Puchitto.Server.Realms;

/// <summary>
/// A singular realm slot.
/// </summary>
public class RealmSlot
{
    /// <summary>
    /// The realm loading task.
    /// </summary>
    public required Lazy<Task<Realm>> RealmTask { get; init; }
    
    /// <summary>
    /// The realm tick cancellation source.
    /// </summary>
    public CancellationTokenSource? RealmTickCancellation { get; set; }
    
    /// <summary>
    /// The current state of the realm.
    /// </summary>
    public RealmState State { get; set; } = RealmState.Loading;
    
    /// <summary>
    /// The realm ticking task.
    /// </summary>
    public Task? RealmTickingTask { get; set; }
}