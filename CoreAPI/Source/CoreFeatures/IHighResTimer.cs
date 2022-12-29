using VaultCore.CoreAPI;

namespace VaultCore.Features;

/// <summary>
/// Feature that can provide High Resolution timer related functionality to cores
/// </summary>
public interface IHighResTimer : IVaultCoreFeature<IHighResTimer.FeatureApi>
{
    public interface FeatureApi : IVaultCoreFeatureApi
    {
        /// <summary>
        /// Samples the High Resolution Timer and returns the number of ticks since program start
        /// </summary>
        ulong HighResolutionTimerSample { get; }
        
        /// <summary>
        /// Returns number of ticks per second of the High Resolution Timer
        /// </summary>
        ulong HighResolutionTimerSampleFrequency { get; }
        
        /// <summary>
        /// The total seconds since the program started 
        /// </summary>
        double TimeSinceStartup { get; }
    }
}