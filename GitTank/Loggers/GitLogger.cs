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
                    $"{DirectoryPath}/{path}-.json",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .CreateLogger();

            Log = logger;
        }
    }
}
