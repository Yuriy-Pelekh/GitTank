using Serilog;
using Serilog.Events;

namespace GitTank.Loggers
{
    internal class GeneralLogger : BaseLogger
    {
        private const string LogTemplate = "[{Timestamp:HH:mm:ss} {Level:u}] {Message:lj}{NewLine}{Exception}";

        public GeneralLogger()
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(configuration =>
                {
                    configuration.WriteTo.File(
                        $"{DirectoryPath}/.log",
                        outputTemplate: LogTemplate,
                        rollingInterval: RollingInterval.Day);
                })
                .WriteTo.Logger(configuration =>
                {
                    configuration.WriteTo.File(
                        $"{DirectoryPath}/Errors-.log",
                        LogEventLevel.Error,
                        LogTemplate,
                        rollingInterval: RollingInterval.Day);
                })
                .CreateLogger();
            Log = logger;
        }
    }
}
