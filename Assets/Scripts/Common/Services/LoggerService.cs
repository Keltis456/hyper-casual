using Common.Interfaces;
using UnityEngine;
using ILogger = Common.Interfaces.ILogger;

namespace Common.Services
{
    public class LoggerService : ILogger
    {
        private readonly IConfigurationService _configService;
        private const string LOG_PREFIX = "[Game]";

        public bool IsEnabled { get; private set; }

        public LoggerService(IConfigurationService configService)
        {
            _configService = configService;
            IsEnabled = _configService?.Config?.enableDebugLogs ?? true;
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!IsEnabled && level < LogLevel.Warning) return;

            var formattedMessage = $"{LOG_PREFIX} {message}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);
    }
}
