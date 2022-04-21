using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Loggers
{
    public class GitLogger : ITankLogger
    {
        private readonly ILogger _logger;
        private readonly string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString();

        public GitLogger(string path)
        {
            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{directoryPath}/logs/{path}.txt", outputTemplate: "[{Timestamp:HH:mm:ss}] {Message:l} {NewLine}{Exception}")
                .CreateLogger();

            _logger = logger;
        }

        public void LogDebug(string message)
        {
                _logger.Debug(message);
        }

        public void LogError(string message, Exception exception)
        {
                _logger.Error(exception,message);
        }

        public void LogInformation(string message)
        {
                _logger.Information(message);
        }

        public void LogWarning(string message)
        {
                _logger.Warning(message);
        }
    }
}
