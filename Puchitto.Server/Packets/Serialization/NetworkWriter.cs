using System.Text;

namespace Puchitto.Server.Packets.Serialization;

/// <summary>
/// A network buffer writer.
/// </summary>
public ref struct NetworkWriter
{
    /// <summary>
    /// Gets the position of the writer.
    /// </summary>
    public int Position => _offset;
    
    /// <summary>
    /// The buffer we're writing to.
    /// </summary>
    private readonly Span<byte> _writeBuffer;
    
    /// <summary>
    /// The offset.
    /// </summary>
    private int _offset;
    
    /// <summary>
    /// Constructs a new network writer.
    /// </summary>
    /// <param name="buffer">The buffer we're writing to.</param>
    /// <param name="offset">The offset into the buffer.</param>
    public NetworkWriter(Span<byte> buffer, int offset)
    {
        _writeBuffer = buffer;
        _offset = offset;
    }

    /// <summary>
    /// Writes a 32-bit signed integer.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteInt32(int value)
    {
        _writeBuffer[_offset++] = (byte)value;
        _writeBuffer[_offset++] = (byte)(value >> 8);
        _writeBuffer[_offset++] = (byte)(value >> 16);
        _writeBuffer[_offset++] = (byte)(value >> 24);
    }
    
    /// <summary>
    /// Writes a single byte.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteByte(byte value)
    {
        _writeBuffer[_offset++] = value;
    }

    /// <summary>
    /// Writes a boolean value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteBoolean(bool value)
    {
        _writeBuffer[_offset++] = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Writes a string.
    /// </summary>
    /// <param name="value">The string value.</param>
    public void WriteString(string value)
    {
        var length = Encoding.UTF8.GetByteCount(value);
        WriteInt32(length);
        
        // TODO: Not performant!
        var bytes = Encoding.UTF8.GetBytes(value);
        foreach (var b in bytes)
        {
            WriteByte(b);
        }
    }
}