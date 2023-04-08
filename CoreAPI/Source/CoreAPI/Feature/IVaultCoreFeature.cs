namespace VaultCore.CoreAPI;
/// <summary>
/// A Vault Core Feature is an interface that provides an Api that the frontend should provide to the core
/// The core can mark which features it needs from the frontend with the use of the 'VaultCoreUsesFeature' Attribute
/// The frontend can provide an implementation of the feature's interface, which is then passed to the core on initialization
/// </summary>
public interface IVaultCoreFeature
{
    /// <summary>
    /// Callback called by the core during Initialisation when this feature is acquired to be used by the core
    /// </summary>
    public void OnCoreAcquiresFeature(Type coreType, Type featureType, List<Type> allCoreFeaturesNeeded);
    
    /// <summary>
    /// Callback called by the core during Shutdown when this feature is released from use by the core
    /// </summary>
    public void OnCoreReleasesFeature(Type coreType, Type featureType, List<Type> allCoreFeaturesNeeded);
}