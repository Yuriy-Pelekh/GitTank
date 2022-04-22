using System;

namespace GitTank.Loggers
{
    internal interface ILogger
    {
        void LogDebug(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message, Exception exception);
    }
}
