using Serilog;
using Serilog.Events;

namespace GitTank.Loggers
{
    internal class GeneralLogger : BaseLogger
    {
        private const string LogTemplate = "[{Timestamp:HH:mm:ss} {Level:u} {SourceContext}] {Message:lj}{NewLine}{EscapedException}";

        public GeneralLogger()
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(configuration =>
                {
                    configuration.WriteTo.File(
                        $"{DirectoryPath}/.log",
                        outputTemplate: LogTemplate,
                        rollingInterval: RollingInterval.Day)
                    .Enrich.FromLogContext();
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
