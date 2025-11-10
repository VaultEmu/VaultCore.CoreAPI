namespace VaultCore.CoreAPI;

/// <summary>
///     Main Base class that should be inherited from to create an emulation core that vault can use
/// </summary>
public abstract class VaultCoreBase
{
    private readonly Dictionary<Type, IVaultCoreFeature> _featureImpl = new();

    /// <summary>
    ///     Sets the fixed rate in Ms at which the core should have Update() called. If set to 0 then the core will be called
    ///     as fast as possible
    ///     If set to low value, the frontend may call update multiple times a frame to catch up.
    /// </summary>
    public virtual float FixedUpdateRateMs => 0;

    /// <summary>
    ///     If MaxNumFixedUpdatesInOneFrame is set to a none-zero value, then this controls the number of
    ///     Update() calls the frontend can call a frame to catch up if needed to achieve FixedUpdateRateMs.
    ///     This can stop the "Spiral of death" if your fixed updates take longer then the time needed
    ///     If set to 0, then no limit is placed on the updates
    /// </summary>
    public virtual int MaxNumFixedUpdatesInOneFrame => 0;

    /// <summary>
    ///     Called during Initialise() and should be implemented by the core to initialise it.
    ///     If Initialization fails, or a needed subsystem cannot be resolved, throw an exception
    /// </summary>
    protected abstract void InitialiseCore();

    /// <summary>
    ///     Called to update the Core. Will be called every frame by the frontend unless UpdateRateMs is set to provide a fixed
    ///     update rate
    /// </summary>
    /// <param name="deltaTime">
    ///     time since last update (will be a fixed value of (1 / UpdateRateMs) if UpdateRateMs is set to a
    ///     non-zero value)
    /// </param>
    public abstract void Update(float deltaTime);

    /// <summary>
    ///     Called during Shutdown() and should be implemented by the core to perform any cleanup as needed.
    /// </summary>
    protected abstract void ShutDownCore();

    /// <summary>
    ///     Called when the Core is created to initialise it.
    /// </summary>
    public void Initialise(IVaultCoreFeatureResolver featureResolver)
    {
        AcquireCoreFeatureImplementations(featureResolver);
        InitialiseCore();
    }
    
    /// <summary>
    ///     Called before the core is destroyed to allow the core to clean up
    /// </summary>
    public void ShutDown()
    {
        ShutDownCore();
        ReleaseCoreFeatureImplementations();
    }

    /// <summary>
    ///     Gets the Implementation for a feature this core uses
    /// </summary>
    /// <typeparam name="T">Type of feature implementation to get (should be a type inheriting from IVaultCoreFeature)</typeparam>
    /// <returns>Feature implementation</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if trying to get a feature Implementation for one not acquired when core is initialized
    /// </exception>
    public T GetFeatureImplementation<T>() where T : IVaultCoreFeature
    {
        if(_featureImpl.TryGetValue(typeof(T), out var featureImpl) == false)
        {
            throw new InvalidOperationException($"Trying to get feature impl that was not acquire at startup: {typeof(T)}," +
                                                $"Was the feature added to the VaultCoreUsesFeature attribute?");
        }

        return (T)featureImpl;
    }

    /// <summary>
    ///     Attempts to acquire all the feature implementations needed for the feature of this core
    /// </summary>
    /// <param name="featureResolver">feature resolver class that can get an feature implementation from the frontend</param>
    /// <exception cref="MissingCoreFeatureException">
    ///     Thrown if featureResolver is unable to acquire an feature implementation that is needed by the core
    /// </exception>
    private void AcquireCoreFeatureImplementations(IVaultCoreFeatureResolver featureResolver)
    {
        var coreFeatureInterfaces = GetAllCoreFeaturesUsedByCore();

        foreach (var featureInterface in coreFeatureInterfaces)
        {
            if(_featureImpl.ContainsKey(featureInterface))
            {
                continue;
            }

            var featureImpl = featureResolver.GetCoreFeatureImplementation(featureInterface);

            if(featureImpl == null)
            {
                throw new MissingCoreFeatureException($"Unable to acquire feature implementation needed for Core feature {featureInterface}");
            }

            _featureImpl.Add(featureInterface, featureImpl);
        }

        foreach (var feature in _featureImpl)
        {
            feature.Value.OnCoreAcquiresFeature(GetType(), feature.Key, coreFeatureInterfaces);
        }
    }

    private void ReleaseCoreFeatureImplementations()
    {
        var coreFeatureInterfaces = GetAllCoreFeaturesUsedByCore();

        foreach (var feature in _featureImpl)
        {
            feature.Value.OnCoreReleasesFeature(GetType(), feature.Key, coreFeatureInterfaces);
        }

        _featureImpl.Clear();
    }

    public List<Type> GetAllCoreFeaturesUsedByCore()
    {
        var coreFeatureTypes = new List<Type>();

        var coreFeatureAttributes = GetType().GetCustomAttributes(typeof(VaultCoreUsesFeatureAttribute), true).Cast<VaultCoreUsesFeatureAttribute>().ToList();

        foreach (var coreFeatureAttribute in coreFeatureAttributes)
        {
            coreFeatureTypes.AddRange(coreFeatureAttribute.CoreFeatureTypes);
        }

        return coreFeatureTypes.Distinct().ToList();
    }
}