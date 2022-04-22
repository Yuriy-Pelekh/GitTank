using System;

namespace GitTank.Loggers
{
    public interface ILogger
    {
        void Debug(string message);

        void Information(string message);

        void Warning(string message);

        void Error(string message, Exception exception);
    }
}
