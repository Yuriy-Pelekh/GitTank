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
    class GeneralLogger
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

        public void LogDebug(string message, Exception exception = null)
        {
            if (exception == null)
                _logger.Debug(message);
            else
                _logger.Debug(exception, message);
        }

        public void LogError(string message, Exception exception = null)
        {
            if (exception == null)
                _logger.Error(message);
            else
                _logger.Error(exception, message);
        }

        public void LogInformation(string message, Exception exception = null)
        {
            if (exception == null)
                _logger.Information(message);
            else
                _logger.Information(exception, message);
        }

        public void LogWarning(string message, Exception exception = null)
        {
            if (exception == null)
                _logger.Warning(message);
            else
                _logger.Warning(exception, message);
        }
    }
}