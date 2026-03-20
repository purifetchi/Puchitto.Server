namespace Puchitto.Server.Data.Alf;

/// <summary>
/// Exception thrown when we're trying to read an invalid ALF package.
/// </summary>
internal class InvalidPackageException : Exception
{
    /// <summary>
    /// Construct a new invalid package exception.
    /// </summary>
    /// <param name="path">The path to the package.</param>
    public InvalidPackageException(string path)
        : base($"{path} is an invalid ALF package!")
    {

    }
}