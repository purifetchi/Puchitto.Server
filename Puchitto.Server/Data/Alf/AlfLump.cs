namespace Puchitto.Server.Data.Alf;

/// <summary>
/// A lump.
/// </summary>
public class AlfLump
{
    /// <summary>
    /// The checksum.
    /// </summary>
    public uint Checksum { get; private set; }

    /// <summary>
    /// The size.
    /// </summary>
    public uint Size { get; private set; }

    /// <summary>
    /// The offset within the file.
    /// </summary>
    public long Pointer { get; private set; }
        
    /// <summary>
    /// The path of this file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Reads a lump from a binary reader.
    /// </summary>
    /// <param name="br">The binary reader.</param>
    public AlfLump(BinaryReader br)
    {
        Checksum = br.ReadUInt32();
        Size = br.ReadUInt32();
        Pointer = br.ReadInt64();
        Path = br.ReadString();
    }
}