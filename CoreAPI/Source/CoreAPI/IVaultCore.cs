namespace VaultCore.CoreAPI;

/// <summary>
/// Main Interface to implement to create an emulation core that vault can use
/// </summary>
public interface IVaultCore
{
    /// <summary>
    /// Sets the fixed rate in Ms at which the core should have update() called. If set to 0 then the core will be updated as fast as possible
    /// If set to low value, the frontend may call update multiple times to catch up.
    /// </summary>
    float UpdateRateMs => 0;
    
    /// <summary>
    /// If UpdateRateMs is set to a none-zero value, then this controls the number of Update calls the frontend can call to catch up if needed.
    /// This can stop the classic "Spiral of death" if your updates take longer then the time needed
    /// If set to 0, then no limit is placed on the updates
    /// </summary>
    int maxNumUpdates => 0;
    
    /// <summary>
    /// Called when the Core is created to initialise it.
    /// If Initialization fails, or a needed subsystem cannot be resolved, throw an exception 
    /// </summary>
    /// <param name="featureResolver">Use this IFeatureResolve to retrieve subsystems from the frontend that are needed by this core.</param>
    void Initialise(IFeatureResolver featureResolver);
    
    /// <summary>
    /// Called to update the Core. Will be called every frame by the frontend unless UpdateRateMs is set to provide a fixed update rate
    /// </summary>
    /// <param name="deltaTime">time since last update (will be a fixed value of (1 / UpdateRateMs) if UpdateRateMs is set to a non-zero value)</param>
    void Update(float deltaTime);

    /// <summary>
    /// Called before the core is destroyed to allow the core to clean up
    /// </summary>
    void ShutDown();
}