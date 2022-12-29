namespace VaultCore.CoreAPI;

/// <summary>
/// Implements functionality to allow a core to request access to a features Api implementation from the frontend
/// </summary>
public interface IVaultCoreFeatureApiResolver
{
    //Gets the concrete feature Api that implements an interface that derives from IVaultCoreFeatureApi.
    //Will return null if the feature Api is not provided by the frontend
    public IVaultCoreFeatureApi? GetCoreFeatureApiImplementation(Type vaultCoreApiType);
}