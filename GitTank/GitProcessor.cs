using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;
using Serilog.Context;
using System.Diagnostics;
using GitTank.Models;

namespace GitTank
{
    internal class GitProcessor
    {
        public event OutputEventHandler Output;
        private readonly ProcessHelper _processHelper;
        private const string Command = "git";
        private string _rootWorkingDirectory;
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;
        private List<Sources> _sources;

        public GitProcessor(IConfiguration configuration, ILogger logger)
        {
            LogContext.PushProperty(Constants.SourceContext, GetType().Name);
            _processHelper = new ProcessHelper(logger);
            _processHelper.Output += OnOutput;

            _sources = configuration.GetSection("appSettings").GetSection("sources").Get<List<Sources>>();
            var t = GetWorkingDirectoryByRepositoryName("GitTank-Test2");

            _repositories = _sources.SelectMany(c => c.Repositories).ToList();
            logger.Debug($"Repositories: {string.Join(", ", _repositories)}");

            _defaultRepository = configuration.GetValue<string>("appSettings:defaultRepository");
            logger.Debug($"Default repository: {_defaultRepository}");

            _defaultBranch = configuration.GetValue<string>("appSettings:defaultBranch");
            logger.Debug($"Default branch: {_defaultBranch}");
        }

        ~GitProcessor()
        {
            _processHelper.Output -= OnOutput;
        }

        private string GetWorkingDirectoryByRepositoryName(string repositoryName)
        {
            string workingDirectory = null;
            foreach(var r in _sources)
            {
                if (r.Repositories.Contains(repositoryName))
                {
                    workingDirectory = Path.GetFullPath(r.SourcePath);
                    break;
                }
            }
            return workingDirectory;
        }

        private void OnOutput(string line)
        {
            Output?.Invoke(line);
        }

        public async Task<string> GetBranch()
        {
            const string arguments = "rev-parse --abbrev-ref HEAD"; //"git branch --show-current";

            var result = string.Empty;

            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, arguments, workingDirectory);
                var currentBranch = await _processHelper.Execute();
                if (string.Equals(repository, _defaultRepository))
                {
                    result = currentBranch;
                }
            }

            return result;
        }

        public async Task Update()
        {
            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string[] arguments =
                {
                    //"fetch -v --progress --prune \"origin\"",
                    "pull --progress -v --no-rebase \"origin\"",
                    //"remote prune origin"
                };

                foreach (var argument in arguments)
                {
                    _processHelper.Configure(Command, argument, workingDirectory);
                    await _processHelper.Execute();
                }
            }
        }

        public async Task<string> Branches()
        {
            const string arguments = "branch"; // -r - only remote, -a - all
            _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(_defaultRepository);
            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);

            _processHelper.Configure(Command, arguments, workingDirectory);
            return await _processHelper.Execute();
        }

        public async Task<string> GetAllBranches(string repositoryPath)
        {
            const string arguments = "branch -a"; // -r - only remote, -a - all
            _processHelper.Configure(Command, arguments, repositoryPath);
            return await _processHelper.Execute();
        }

        public async Task Checkout(string selectedItem)
        {
            var remoteBranch = "ls-remote --heads origin {0}";
            var localBranch = "branch --list {0}";
            var arguments = $"checkout -b {selectedItem}";

            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, string.Format(localBranch, selectedItem), workingDirectory);
                var localBranchExists = await _processHelper.Execute();
                _processHelper.Configure(Command, string.Format(remoteBranch, selectedItem), workingDirectory);
                var remoteBranchExists = await _processHelper.Execute();

                if (!string.IsNullOrWhiteSpace(localBranchExists) || !string.IsNullOrWhiteSpace(remoteBranchExists))
                {
                    arguments = $"checkout {selectedItem}";
                }

                _processHelper.Configure(Command, arguments, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task Sync()
        {
            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string[] arguments =
                {
                    "fetch -v --progress --prune \"origin\"",
                    $"merge origin/{_defaultBranch}"
                };

                foreach (var argument in arguments)
                {
                    _processHelper.Configure(Command, argument, workingDirectory);
                    await _processHelper.Execute();
                }
            }
        }

        public async Task Push()
        {
            // Requires: git config --global push.default current
            const string arguments = "push -u";

            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, arguments, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task OpenTerminal(string selectedRepository)
        {
            _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(selectedRepository);
            var workingDirectory = Path.Combine(_rootWorkingDirectory, selectedRepository);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,

                    WorkingDirectory = workingDirectory,
                    FileName = @"C:\Program Files\Git\git-bash.exe",
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }

        public async Task Fetch()
        {
            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string argument = "fetch -v --progress --prune \"origin\"";

                _processHelper.Configure(Command, argument, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task CreateBranch(string newBranch)
        {
            foreach (var repository in _repositories)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repository);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string[] arguments =
                {
                    "pull --progress -v --no-rebase \"origin\"",
                    $"checkout -b {newBranch}",
                    $"push -u origin {newBranch}"
                };

                foreach (var argument in arguments)
                {
                    _processHelper.Configure(Command, argument, workingDirectory);
                    await _processHelper.Execute();
                }
            }
        }
    }
}
