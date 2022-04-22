using System;
using System.IO;

namespace GitTank.Loggers
{
    public abstract class BaseLogger : ILogger
    {
        protected Serilog.ILogger Log;
        protected readonly string DirectoryPath;

        protected BaseLogger(string path = null)
        {
            DirectoryPath = path ?? Path.Combine(Directory.GetCurrentDirectory(), "logs");
        }

        public void LogDebug(string message)
        {
            Log.Debug(message);
        }

        public void LogError(string message, Exception exception)
        {
            Log.Error(exception, message);
        }

        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        public void LogWarning(string message)
        {
            Log.Warning(message);
        }
    }
}
