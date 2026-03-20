using System.IO.MemoryMappedFiles;

namespace Puchitto.Server.Data.Alf;

/// <summary>
/// An ALF package file.
/// </summary>
public class AlfPackage : IDisposable
{
    /// <summary>
    /// The memory mapped ALF package file.
    /// </summary>
    private readonly MemoryMappedFile _alfFile;

    /// <summary>
    /// Whether this package was disposed of.
    /// </summary>
    private bool _disposedValue;

    /// <summary>
    /// The header.
    /// </summary>
    public AlfHeader Header { get; private set; }

    /// <summary>
    /// The list of lumps.
    /// </summary>
    public IReadOnlyList<AlfLump> Lumps { get; private set; }

    /// <summary>
    /// The path to the file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Creates a new package from a file path.
    /// </summary>
    /// <param name="path">The file path</param>
    public AlfPackage(string path)
    {
        Path = path;
        _alfFile = MemoryMappedFile.CreateFromFile(path);

        ParseHeader();
    }

    /// <summary>
    /// Parses the header.
    /// </summary>
    private void ParseHeader()
    {
        using var accessor = _alfFile.CreateViewAccessor();
        accessor.Read(0, out AlfHeader header);
            
        if (!header.ValidateHeader())
            throw new InvalidPackageException(Path);

        Header = header;
        ParseFileDirectory();
    }

    /// <summary>
    /// Parses the file directory table.
    /// </summary>
    private void ParseFileDirectory()
    {
        var lumps = new List<AlfLump>();

        using var stream = _alfFile.CreateViewStream(Header.Pointer, 0L);
        using var br = new BinaryReader(stream);
                
        for (var i = 0; i < Header.Count; i++)
            lumps.Add(new AlfLump(br));

        Lumps = lumps;
    }

    /// <summary>
    /// Reads a byte array from an alf lump.
    /// </summary>
    /// <param name="lump">The lump.</param>
    /// <returns>The read byte array.</returns>
    public byte[] Read(AlfLump lump)
    {
        var data = new byte[lump.Size];
        using var accessor = _alfFile.CreateViewAccessor(lump.Pointer, lump.Size);
            
        accessor.ReadArray(0, data, 0, (int)lump.Size);

        return data;
    }

    /// <summary>
    /// Gets a stream for a lump.
    /// </summary>
    /// <param name="lump">The lump.</param>
    /// <returns>The data stream.</returns>
    public Stream GetStream(AlfLump lump)
    {
        return _alfFile.CreateViewStream(lump.Pointer, lump.Size);
    }

    /// <summary>
    /// Disposes the memory mapped file.
    /// </summary>
    /// <param name="disposing">Whether we are disposing right now.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
            
        if (disposing)
            _alfFile.Dispose();

        _disposedValue = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}