﻿using GitTank.Common;
using GitTank.Loggers;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GitTank.Core
{
    internal class ProcessHelper : IDisposable
    {
        public event OutputPerRepositoryEventHandler Output;

        private TaskCompletionSource<bool> _processCompletionSource;
        private int _linesCount;
        private readonly Process _process;
        private readonly StringBuilder _output = new();
        private readonly StringBuilder _jsonOutput = new();
        private readonly StringBuilder _result = new();
        private readonly StringBuilder _detailsOutput = new();
        private readonly int _senderIndex;
        private readonly ILogger _gitLogger;
        private readonly ILogger _generalLogger;

        public ProcessHelper(ILogger logger, string workingDirectory, int senderIndex)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                },
                EnableRaisingEvents = true
            };

            _senderIndex = senderIndex;

            _generalLogger = logger;
            _gitLogger = new GitLogger(Path.GetFileName(workingDirectory));

            _process.OutputDataReceived += OnDataReceived;
            _process.ErrorDataReceived += OnDataReceived;

            _process.Exited += OnExited;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data?.Trim()))
            {
                _output.AppendLine(e.Data);
                _result.AppendLine(e.Data);

                if (_linesCount > 10000)
                {
                    OnOutput(_senderIndex, _output.ToString());
                    _output.Clear();
                    _linesCount = 0;
                }
                else
                {
                    _linesCount++;
                }
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            _processCompletionSource.TrySetResult(true);
        }

        private void ConfigureCommand(string command, string arguments)
        {
            _process.StartInfo.FileName = command;
            _process.StartInfo.Arguments = arguments;
        }

        public async Task<string> Execute(string command, string arguments)
        {
            _output.Clear();
            _jsonOutput.Clear();
            _result.Clear();
            _processCompletionSource = new TaskCompletionSource<bool>();

            ConfigureCommand(command, arguments);

            var commandLog = $"Command executed: {_process.StartInfo.FileName} {_process.StartInfo.Arguments} in {_process.StartInfo.WorkingDirectory}";
            _jsonOutput.Append(commandLog);
            OnOutput(_senderIndex, commandLog + Environment.NewLine);

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            await _processCompletionSource.Task;

            // Throws an exception on some machines. Suspect it is because of some policies configurations. Requires more attention.
            // See
            //     Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            // in App.xaml.cs for reference
            //_process.WaitForExit();
            _process.CancelErrorRead();
            _process.CancelOutputRead();

            if (_output.Length > 0)
            {
                OnOutput(_senderIndex, _output.ToString());
                _detailsOutput.AppendLine(_output.ToString());
                _output.Clear();
                _linesCount = 0;
            }

            LogContext.Reset();
            LogContext.PushProperty("Details", _detailsOutput.ToString());

            var summary = $"{(_process.ExitCode == 0 ? "Success" : "Failed")} ({_process.ExitTime - _process.StartTime} @ {_process.ExitTime.ToLocalTime()})";
            OnOutput(_senderIndex, summary);
            LogContext.PushProperty("Status", summary);
            _gitLogger.Information(_jsonOutput.ToString());
            LogContext.Reset();
            LogContext.PushProperty(Constants.SourceContext, GetType().Name);
            _generalLogger.Information(commandLog + "-" + summary);
            OnOutput(_senderIndex, "_________________________________________________________________________________");

            return _result.ToString();
        }

        private void ReleaseUnmanagedResources()
        {
            _process.OutputDataReceived -= OnDataReceived;
            _process.ErrorDataReceived -= OnDataReceived;

            _process.Exited -= OnExited;
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

        protected virtual void OnOutput(int senderIndex, string line)
        {
            Output?.Invoke(senderIndex, line);
        }
    }
}
