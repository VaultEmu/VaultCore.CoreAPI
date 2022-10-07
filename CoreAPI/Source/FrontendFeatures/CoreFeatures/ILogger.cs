namespace VaultCore.CoreAPI;

//Simple interface for a logging feature. Should be provided by all frontends for basic core logging
public interface ILogger : IFeature
{
    //Logs general message
    public void Log(string message);
    
    //Logs a Debug Message
    public void LogDebug(string message, Exception? exception = null);
    
    //Logs Warning at Warn Level
    public void LogWarning(string message, Exception? exception = null);
    
    //Log Message at Error Level
    public void LogError(string message, Exception? exception = null);
    
    //Log Message at Fatal Level (Fatal errors include callstack)
    public void LogFatal(string message, Exception? exception = null, bool showStackTrace = true);
}