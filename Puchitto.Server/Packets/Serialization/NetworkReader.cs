using System.Text;

namespace Puchitto.Server.Packets.Serialization;

public ref struct NetworkReader
{
    /// <summary>
    /// Gets the position of the reader.
    /// </summary>
    public int Position => _offset;
    
    /// <summary>
    /// The buffer we're reading from.
    /// </summary>
    private readonly Span<byte> _reader;
    
    /// <summary>
    /// The offset.
    /// </summary>
    private int _offset;
    
    public NetworkReader(Span<byte> reader, int offset)
    {
        _reader = reader;
        _offset = offset;
    }

    /// <summary>
    /// Reads a 32-bit signed integer.
    /// </summary>
    /// <returns>The 32-bit signed integer.</returns>
    public int ReadInt32()
    {
        var value = _reader[_offset] |
                    (_reader[_offset + 1] << 8) |
                    (_reader[_offset + 2] << 16) |
                    (_reader[_offset + 3] << 24);
        
        _offset += 4;
        return value;
    }

    /// <summary>
    /// Reads a single byte.
    /// </summary>
    /// <returns>The byte.</returns>
    public byte ReadByte()
    {
        return _reader[_offset++];
    }

    /// <summary>
    /// Reads a boolean value.
    /// </summary>
    /// <returns>True if the byte is non-zero; otherwise false.</returns>
    public bool ReadBoolean()
    {
        return _reader[_offset++] != 0;
    }

    /// <summary>
    /// Reads a string.
    /// </summary>
    /// <returns>The string value.</returns>
    public string ReadString()
    {
        var length = ReadInt32();
        
        var stringSpan = _reader.Slice(_offset, length);
        _offset += length;
        
        return Encoding.UTF8.GetString(stringSpan);
    }
}