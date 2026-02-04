namespace Common.Interfaces
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public interface ILogger
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        bool IsEnabled { get; }
    }
}
