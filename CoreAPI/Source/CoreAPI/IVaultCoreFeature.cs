namespace VaultCore.CoreAPI;

/// <summary>
/// A Vault Core Feature is an interface that marks a core as requiring some functionality from the frontend.
/// Its is made up of two interfaces:
///     - An interface inheriting from IVaultCoreFeature which should be added to the core to mark it as needing this feature
///     - An interface inheriting from IVaultCoreFeatureApi which should be set as the Type Parameter. This should have an
///       implementation in the front end that is passed into Initialise when the VaultCore is created
/// </summary>
public interface IVaultCoreFeature<TVaultCoreFeatureAPI> where TVaultCoreFeatureAPI : IVaultCoreFeatureApi
{
}

