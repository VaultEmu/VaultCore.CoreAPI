namespace VaultCore.CoreAPI;

/// <summary>
/// Exception thrown when a core trys to resolve and core feature and is unable too
/// </summary>
public class MissingCoreFeatureException : Exception
{
    public MissingCoreFeatureException()
    {
    }

    public MissingCoreFeatureException(string message)
        : base(message)
    {
    }

    public MissingCoreFeatureException(string message, Exception inner)
        : base(message, inner)
    {
    }
}