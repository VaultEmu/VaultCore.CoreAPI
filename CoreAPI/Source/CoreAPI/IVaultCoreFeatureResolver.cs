namespace VaultCore.CoreAPI;

/// <summary>
/// Implements functionality to allow a core to request access to a feature's implementation from the frontend
/// </summary>
public interface IVaultCoreFeatureResolver
{
    //Gets the concrete feature Api that implements an interface that derives from IVaultCoreFeature.
    //Will return null if the feature Api is not provided by the frontend
    public IVaultCoreFeature? GetCoreFeatureImplementation(Type vaultCoreApiType);
}