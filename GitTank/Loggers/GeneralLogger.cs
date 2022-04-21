using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Loggers
{
    class GeneralLogger : ITankLogger
    {
        const string logTemplate = "[{Timestamp:HH:mm:ss}] {Message:l} {NewLine}{Exception}";
        private readonly ILogger _logger;
        private readonly string directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString();
        public GeneralLogger()
        {
            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(l =>
                {
                    l.WriteTo.File($"{directoryPath}/logs/generalLog.txt", outputTemplate: logTemplate);
                })
                .WriteTo.Logger(l =>
                {
                     l.WriteTo.File($"{directoryPath}/logs/errorLog.txt", LogEventLevel.Error, logTemplate);
                })
                .CreateLogger();
            _logger = logger;
        }

        public void LogDebug(string message)
        {
                _logger.Debug(message);
        }

        public void LogError(string message, Exception exception)
        {
                _logger.Error(exception, message);
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