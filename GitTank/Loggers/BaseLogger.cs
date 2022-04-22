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

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Error(string message, Exception exception)
        {
            Log.Error(exception, message);
        }

        public void Information(string message)
        {
            Log.Information(message);
        }

        public void Warning(string message)
        {
            Log.Warning(message);
        }
    }
}
