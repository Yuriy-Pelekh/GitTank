using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Loggers
{
    interface ITankLogger
    {
        void LogDebug(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message, Exception exception);
    }
}
