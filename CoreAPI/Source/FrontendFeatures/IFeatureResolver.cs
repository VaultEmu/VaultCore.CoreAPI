namespace VaultCore.CoreAPI;

/// <summary>
/// Implements functionality to allow a core to request access to features provided by the frontend
/// that the core needs to function
/// </summary>
public interface IFeatureResolver
{
    //Gets the concrete feature that implements an interface that derives from IFeature.
    //Will return null if the feature is not provided by the frontend
    public T GetFeature<T>() where T : IFeature;
}