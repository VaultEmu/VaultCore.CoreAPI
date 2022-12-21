namespace VaultCore.CoreAPI;

/// <summary>
/// Simple interface for a logging feature. Should be provided by all frontends for basic core logging at diffrent levels
/// </summary>
public interface ILogger : IFeature
{
    /// <summary>
    /// Logs a general message
    /// </summary>
    /// <param name="message">Message text to log</param>
    public void Log(string message);
    
    /// <summary>
    /// Logs a Message and/or exception at Debug Level
    /// </summary>
    /// <param name="message">Message text to log</param>
    /// <param name="exception">Exception to log</param>
    public void LogDebug(string message, Exception? exception = null);
    
    /// <summary>
    /// Logs a message and/or exception at Warning Level
    /// </summary>
    /// <param name="message">Message text to log</param>
    /// <param name="exception">Exception to log</param>
    public void LogWarning(string message, Exception? exception = null);
    
    /// <summary>
    /// Log Message and/or exception at Error Level 
    /// </summary>
    /// <param name="message">Message text to log</param>
    /// <param name="exception">Exception to log</param>
    public void LogError(string message, Exception? exception = null);
    
    /// <summary>
    /// Log Message and/or exception at Fatal Level (Fatal errors include callstack)
    /// </summary>
    /// <param name="message">Message text to log</param>
    /// <param name="exception">Exception to log</param>
    /// <param name="showStackTrace">if true, then stacktrace is shown in the message</param>
    public void LogFatal(string message, Exception? exception = null, bool showStackTrace = true);
}