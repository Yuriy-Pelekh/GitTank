using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Loggers
{
    interface ITankLogger
    {
        void LogDebug(string message, Exception exception);

        void LogInformation(string message, Exception exception = null);

        void LogWarning(string message, Exception exception = null);

        void LogError(string message, Exception exception = null);
    }
}
