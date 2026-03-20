using System.Numerics;
using Puchitto.Server.Clients;

namespace Puchitto.Server.Packets.Serialization.Facades;

/// <summary>
/// A class facade over the ref struct <see cref="NetworkWriter"/>.
/// </summary>
public class WriterFacade
{
    /// <summary>
    /// Gets or sets the backing array.
    /// </summary>
    public byte[] BackingArray { get; set; }
    
    /// <summary>
    /// The client we're sending to.
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// Gets the amount of bytes written so far.
    /// </summary>
    public int Written => _offset;

    /// <summary>
    /// The current write offset within the backing array.
    /// </summary>
    private int _offset;

    /// <summary>
    /// Constructs a new <see cref="WriterFacade"/>.
    /// </summary>
    /// <param name="backingArray">The backing byte array.</param>
    /// <param name="client">The client we're sending to.</param>
    /// <param name="offset">The initial offset to start writing at.</param>
    public WriterFacade(
        byte[] backingArray,
        Client client,
        int offset = 0)
    {
        BackingArray = backingArray;
        Client = client;
        _offset = offset;
    }

    /// <summary>
    /// Writes a 32-bit signed integer.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteInt32(int value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteInt32(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a 32-bit floating-point number.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteFloat(float value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteFloat(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a Vector3.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteVector3(Vector3 value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteVector3(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a Quaternion.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteQuaternion(Quaternion value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteQuaternion(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a single byte.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteByte(byte value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteByte(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a boolean value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteBoolean(bool value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteBoolean(value);
        _offset = writer.Position;
    }

    /// <summary>
    /// Writes a string.
    /// </summary>
    /// <param name="value">The string value.</param>
    public void WriteString(string value)
    {
        var writer = new NetworkWriter(BackingArray, _offset);
        writer.WriteString(value);
        _offset = writer.Position;
    }
}