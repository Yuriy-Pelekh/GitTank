using Serilog;
using Serilog.Formatting.Json;
using System;

namespace GitTank.Loggers
{
    public class GitLogger : BaseLogger
    {
        public GitLogger(string path)
        {
            const string outputTemplate = "[{Timestamp:HH:mm:ss}] {Message:l} {NewLine}{Exception}";

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    $"{DirectoryPath}/{path}-.log",
                    outputTemplate: outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .WriteTo.File(new JsonFormatter(), $"{DirectoryPath}/{path}-{DateTime.Now.ToString("yyyyMMdd")}.json",
                 shared:true)
                .CreateLogger();

            Log = logger;
        }
    }
}
