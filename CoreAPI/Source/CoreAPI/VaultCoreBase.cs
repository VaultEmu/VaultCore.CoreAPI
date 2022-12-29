namespace VaultCore.CoreAPI;

/// <summary>
///     Main Base class that should be inherited from to create an emulation core that vault can use
/// </summary>
public abstract class VaultCoreBase
{
    /// <summary>
    ///     Sets the fixed rate in Ms at which the core should have update() called. If set to 0 then the core will be updated
    ///     as fast as possible
    ///     If set to low value, the frontend may call update multiple times to catch up.
    /// </summary>
    public virtual float UpdateRateMs => 0;

    /// <summary>
    ///     If UpdateRateMs is set to a none-zero value, then this controls the number of Update calls the frontend can call to
    ///     catch up if needed.
    ///     This can stop the classic "Spiral of death" if your updates take longer then the time needed
    ///     If set to 0, then no limit is placed on the updates
    /// </summary>
    public virtual int maxNumUpdates => 0;

    /// <summary>
    ///     Called during Initialise() and should be implemented by the core to initialise it.
    ///     If Initialization fails, or a needed subsystem cannot be resolved, throw an exception
    /// </summary>
    protected abstract void InitialiseImpl();

    /// <summary>
    ///     Called during Update() and should be implemented by the core to perform updating.
    ///     Will be called every frame by the frontend unless UpdateRateMs is set to provide a fixed update rate
    /// </summary>
    /// <param name="deltaTime">
    ///     time since last update (will be a fixed value of (1 / UpdateRateMs) if UpdateRateMs is set to a
    ///     non-zero value)
    /// </param>
    protected abstract void UpdateImpl(float deltaTime);

    /// <summary>
    ///     Called during Shutdown() and should be implemented by the core to perform any cleanup as needed.
    /// </summary>
    protected abstract void ShutDownImpl();

    private readonly Dictionary<Type, IVaultCoreFeature> _featureApiImpl = new();

    /// <summary>
    ///     Called when the Core is created to initialise it.
    /// </summary>
    public void Initialise(IVaultCoreFeatureResolver featureResolver)
    {
        AcquireCoreFeatureApiImplementations(featureResolver);
        InitialiseImpl();
    }

    /// <summary>
    ///     Called to update the Core. Will be called every frame by the frontend unless UpdateRateMs is set to provide a fixed
    ///     update rate
    /// </summary>
    /// <param name="deltaTime">
    ///     time since last update (will be a fixed value of (1 / UpdateRateMs) if UpdateRateMs is set to a
    ///     non-zero value)
    /// </param>
    public void Update(float deltaTime)
    {
        UpdateImpl(deltaTime);
    }

    /// <summary>
    ///     Called before the core is destroyed to allow the core to clean up
    /// </summary>
    public void ShutDown()
    {
        ShutDownImpl();
    }

    /// <summary>
    ///     Gets the Api Implementation for a feature this core uses
    /// </summary>
    /// <typeparam name="T">Type of Api implementation to get (should be a type inheriting from IVaultCoreFeature)</typeparam>
    /// <returns>Api implementation</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if trying to get Api Implementation for one not acquired when core is initialized
    /// </exception>
    public T GetFeatureApi<T>() where T : IVaultCoreFeature
    {
        if(_featureApiImpl.TryGetValue(typeof(T), out var apiImpl) == false)
        {
            throw new InvalidOperationException($"Trying to get feature impl that was not acquire at startup: {typeof(T)}");
        }

        return (T)apiImpl;
    }

    /// <summary>
    ///     Attempts to acquire all the api implementations needed for the feature of this core
    /// </summary>
    /// <param name="featureResolver">feature api resolver class that can get an api implementation from the frontend</param>
    /// <exception cref="MissingCoreFeatureApiException">
    ///     Thrown if featureResolver is unable to acquire an api implementation
    /// </exception>
    private void AcquireCoreFeatureApiImplementations(IVaultCoreFeatureResolver featureResolver)
    {
        var coreFeatureInterfaces = GetAllCoreFeaturesUsedByCore();
        
        foreach (var featureInterface in coreFeatureInterfaces)
        {
            if(_featureApiImpl.ContainsKey(featureInterface))
            {
                continue;
            }

            var featureImpl = featureResolver.GetCoreFeatureImplementation(featureInterface);

            if(featureImpl == null)
            {
                throw new MissingCoreFeatureApiException($"Unable to acquire feature implementation needed for Core feature {featureInterface}");
            }

            _featureApiImpl.Add(featureInterface, featureImpl);
            
        }
    }
    
    public List<Type> GetAllCoreFeaturesUsedByCore()
    {
        var coreFeatureTypes = new List<Type>();
        
        var coreFeatureAttributes = GetType().GetCustomAttributes(typeof(VaultCoreUsesFeatureAttribute), true).Cast<VaultCoreUsesFeatureAttribute>().ToList();
        
        foreach(var coreFeatureAttribute in coreFeatureAttributes)
        {
            coreFeatureTypes.AddRange(coreFeatureAttribute.CoreFeatureTypes);
        }
        return coreFeatureTypes.Distinct().ToList();
    }
}