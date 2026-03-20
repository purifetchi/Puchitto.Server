namespace Puchitto.Server.Data.Alf;

/// <summary>
/// The ALF header.
/// </summary>
public readonly struct AlfHeader
{
    public readonly byte Header1;
    public readonly byte Header2;
    public readonly byte Header3;
    public readonly byte Header4;

    public readonly int Flags;
    public readonly int Count;
    public readonly int Pointer;

    /// <summary>
    /// Validates whether this header is a proper ALF header.
    /// </summary>
    /// <returns>Whether this header is valid.</returns>
    public bool ValidateHeader()
    {
        return Header1 == 'K' &&
               Header2 == 'A' &&
               Header3 == 'I' &&
               Header4 == '!';
    }
}