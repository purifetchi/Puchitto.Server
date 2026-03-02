using System.Numerics;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// The transform of the entity.
/// </summary>
public class Transform
{
    /// <summary>
    /// The position of the entity.
    /// </summary>
    public Vector3 Position { get; set; } = Vector3.Zero;

    /// <summary>
    /// The rotation of the entity.
    /// </summary>
    public Quaternion Rotation { get; set; } = Quaternion.Identity;

    /// <summary>
    /// The scale of the entity.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.One;
}