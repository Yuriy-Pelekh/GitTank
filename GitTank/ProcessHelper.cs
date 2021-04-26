using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace GitTank
{
    internal class ProcessHelper: IDisposable
    {
        public event OutputEventHandler Output;
        private readonly Process _process;
        private readonly StringBuilder _output = new();
        private readonly StringBuilder _result = new();
        private int _linesCount;

        public ProcessHelper()
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
                    Output?.Invoke(_output.ToString());
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
        }

        public async Task<string> Execute()
        {
            _output.Clear();
            _result.Clear();
            Output?.Invoke(_process.StartInfo.FileName + " " + _process.StartInfo.Arguments);
            Output?.Invoke("in " + _process.StartInfo.WorkingDirectory + Environment.NewLine);
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _process.WaitForExit();
            _process.CancelErrorRead();
            _process.CancelOutputRead();

            if (_output.Length > 0)
            {
                Output?.Invoke(_output.ToString());
                _output.Clear();
                _linesCount = 0;
            }

            var summary = $"{Environment.NewLine}{(_process.ExitCode == 0 ? "Success" : "Failed")} ({_process.ExitTime - _process.StartTime} @ {_process.ExitTime.ToLocalTime()}){Environment.NewLine}";
            Output?.Invoke(summary);
            Output?.Invoke("_________________________________________________________________________________");
            return _result.ToString();
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
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
    }
}
