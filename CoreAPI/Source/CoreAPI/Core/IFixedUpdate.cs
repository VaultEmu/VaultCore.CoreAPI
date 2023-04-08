namespace VaultCore.CoreAPI;

public interface IFixedUpdate
{
    /// <summary>
    ///     Sets the fixed rate in milliseconds at which the core should have FixedUpdate() called. Should be > 0
    ///     If set to low value, the frontend may call update multiple times to catch up.
    /// </summary>
    public float FixedUpdateRateMs { get; }

    /// <summary>
    ///     If maxNumFixedUpdatesInOneFrame is set to a none-zero value, then this controls the number of
    ///     FixedUpdate() calls the frontend can call to catch up if needed.
    ///     This can stop the "Spiral of death" if your fixed updates take longer then the time needed
    ///     If set to 0, then no limit is placed on the updates
    /// </summary>
    public int maxNumFixedUpdatesInOneFrame { get; }
    
    /// <summary>
    ///     Called to update the Core with a fixed update, with update been called every FixedUpdateRateMs
    /// </summary>
    /// <param name="deltaTime"> time since last update (will be a fixed value of (1 / UpdateRateMs, provided for convenience)</param>
    public void FixedUpdate(float deltaTime);
}