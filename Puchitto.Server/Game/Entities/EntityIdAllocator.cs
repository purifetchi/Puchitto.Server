namespace Puchitto.Server.Game.Entities;

/// <summary>
/// The entity ID allocator.
/// </summary>
public class EntityIdAllocator
{
    /// <summary>
    /// The last assigned ID.
    /// </summary>
    private int _lastId;

    /// <summary>
    /// Constructs a new entity ID allocator.
    /// </summary>
    /// <param name="startFrom">Which id to start from?</param>
    public EntityIdAllocator(int startFrom)
    {
        // TODO: We should be able to return IDs and reuse them.
        _lastId = startFrom;
    }

    /// <summary>
    /// Gets the next ID for this allocator.
    /// </summary>
    /// <returns>
    /// The next ID.
    /// </returns>
    public int GetNextId()
    {
        return Interlocked.Increment(ref _lastId);
    }
}