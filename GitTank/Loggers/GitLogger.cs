using Serilog;
using Serilog.Formatting.Json;

namespace GitTank.Loggers
{
    public class GitLogger : BaseLogger
    {
        public GitLogger(string path)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(new JsonFormatter(),
                    $"{DirectoryPath}/{path}-.log",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .Enrich.FromLogContext()
                .Enrich.With<ExceptionEnricher>()
                .CreateLogger();

            Log = logger;
        }
    }
}
