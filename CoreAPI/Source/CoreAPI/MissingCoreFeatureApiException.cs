namespace VaultCore.CoreAPI;

/// <summary>
/// Exception thrown when a core trys to resolve and core feature and is unable too
/// </summary>
public class MissingCoreFeatureApiException : Exception
{
    public MissingCoreFeatureApiException()
    {
    }

    public MissingCoreFeatureApiException(string message)
        : base(message)
    {
    }

    public MissingCoreFeatureApiException(string message, Exception inner)
        : base(message, inner)
    {
    }
}