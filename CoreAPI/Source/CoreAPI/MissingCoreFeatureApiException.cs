namespace VaultCore.CoreAPI;

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