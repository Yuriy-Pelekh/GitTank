using GitTank.Loggers;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GitTank
{
    internal class ProcessHelper : IDisposable
    {
        public event OutputEventHandler Output;
        private readonly Process _process;
        private readonly StringBuilder _output = new();
        private readonly StringBuilder _jsonOutput = new();
        private readonly StringBuilder _result = new();
        private int _linesCount;
        private ILogger _gitLogger;
        private readonly ILogger _generalLogger;

        public ProcessHelper(ILogger logger)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            _generalLogger = logger;
            _process.OutputDataReceived += OnDataReceived;
            _process.ErrorDataReceived += OnDataReceived;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data?.Trim()))
            {
                _output.AppendLine(e.Data);
                _result.AppendLine(e.Data);
                if (_linesCount > 10000)
                {
                    OnOutput(_output.ToString());
                    _output.Clear();
                    _linesCount = 0;
                }
                else
                {
                    _linesCount++;
                }
            }
        }

        public void Configure(string command, string arguments, string workingDirectory = "")
        {
            _process.StartInfo.FileName = command;
            _process.StartInfo.Arguments = arguments;
            _process.StartInfo.WorkingDirectory = workingDirectory;
            _gitLogger = new GitLogger(Path.GetFileName(workingDirectory));
        }

        public async Task<string> Execute()
        {
            _output.Clear();
            _jsonOutput.Clear();
            _result.Clear();
            LogContext.PushProperty(Constants.SourceContext, GetType().Name);
            var command = $"{_process.StartInfo.FileName} {_process.StartInfo.Arguments}";
            var commandInfo = $"Command executed: {command} in {_process.StartInfo.WorkingDirectory}";
            _generalLogger.Information(commandInfo);
            OnOutput(command);
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            // Throws an exception on some machines. Suspect it is because of some policies configurations. Requires more attention.
            // See
            //     Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            // in App.xaml.cs for reference
            _process.WaitForExit();
            _process.CancelErrorRead();
            _process.CancelOutputRead();

            if (_output.Length > 0)
            {
                OnOutput(_output.ToString());
                _output.Clear();
                _linesCount = 0;
            }
            var summary = $"{(_process.ExitCode == 0 ? "Success" : "Failed")} ({_process.ExitTime - _process.StartTime} @ {_process.ExitTime.ToLocalTime()})";
            _generalLogger.Information(summary);
            OnOutput(summary);
            OnOutput("_________________________________________________________________________________");
            _gitLogger.Information(_jsonOutput.ToString());
            return _result.ToString();
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO: release unmanaged resources here
            _process.OutputDataReceived -= OnDataReceived;
            _process.ErrorDataReceived -= OnDataReceived;
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _process?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ProcessHelper()
        {
            Dispose(false);
        }

        protected virtual void OnOutput(string line)
        {
            Output?.Invoke(line);
            _jsonOutput.AppendLine(line);
        }
    }
}
