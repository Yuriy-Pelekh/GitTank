using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace GitTank.Loggers
{
    internal class GeneralLogger : ITankLogger
    {
        private const string LogTemplate = "[{Timestamp:HH:mm:ss}] {Message:l} {NewLine}{Exception}";
        private readonly ILogger _logger;

        private readonly string _directoryPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString();

        public GeneralLogger()
        {
            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(configuration =>
                {
                    configuration.WriteTo.File($"{_directoryPath}/logs/generalLog.txt", outputTemplate: LogTemplate);
                })
                .WriteTo.Logger(configuration =>
                {
                    configuration.WriteTo.File($"{_directoryPath}/logs/errorLog.txt", LogEventLevel.Error, LogTemplate);
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
