using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;

namespace GitTank
{
    internal class GitProcessor
    {
        public event OutputEventHandler Output;
        private readonly ProcessHelper _processHelper;
        private const string Command = "git";
        private readonly string _rootWorkingDirectory;
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;

        public GitProcessor(IConfiguration configuration, ILogger logger)
        {
            _processHelper = new ProcessHelper(logger);
            _processHelper.Output += OnOutput;

            _rootWorkingDirectory = configuration.GetValue<string>("appSettings:sourcePath");
            logger.Debug($"Original source path: {_rootWorkingDirectory ?? "null"}");

            // Convert path to absolute in case it was set as relative.
            _rootWorkingDirectory = Path.GetFullPath(_rootWorkingDirectory);
            logger.Debug($"Absolute source path: {_rootWorkingDirectory}");

            _defaultRepository = configuration.GetValue<string>("appSettings:defaultRepository");
            logger.Debug($"Default repository: {_defaultRepository}");

            _defaultBranch = configuration.GetValue<string>("appSettings:defaultBranch");
            logger.Debug($"Default branch: {_defaultBranch}");

            _repositories = configuration.GetSection("appSettings:repositories")
                .GetChildren()
                .Select(c => c.Value)
                .ToList();
            logger.Debug($"Repositories: {string.Join(", ", _repositories)}");
        }

        ~GitProcessor()
        {
            _processHelper.Output -= OnOutput;
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
            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);

            _processHelper.Configure(Command, arguments, workingDirectory);
            return await _processHelper.Execute();
        }

        public async Task Checkout(string selectedItem)
        {
            var remoteBranchExistsCommand = "ls-remote --heads origin {0}";
            var localBranchExistsCommand = "branch --list {0}";
            var arguments = $"checkout -b {selectedItem}";

            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, string.Format(localBranchExistsCommand, selectedItem), workingDirectory);
                var localBranchExists = await _processHelper.Execute();
                _processHelper.Configure(Command, string.Format(remoteBranchExistsCommand, selectedItem), workingDirectory);
                var remoteBranchExists = await _processHelper.Execute();

                arguments = !string.IsNullOrWhiteSpace(localBranchExists) || !string.IsNullOrWhiteSpace(remoteBranchExists)
                    ? $"checkout {selectedItem}"
                    : $"checkout -b {selectedItem}";

                _processHelper.Configure(Command, arguments, workingDirectory);
                runningTasks.Add(_processHelper.Execute());
            }

            await Task.WhenAll(runningTasks);
        }

        public async Task Sync()
        {
            foreach (var repository in _repositories)
            {
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
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, arguments, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task Fetch()
        {
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string argument = "fetch -v --progress --prune \"origin\"";

                _processHelper.Configure(Command, argument, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task CreateBranch(string currentBranch, string newBranch)
        {
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string[] arguments =
                {
                    $"checkout {currentBranch}",
                    "pull --progress -v --no-rebase \"origin\"",
                    $"checkout -b {newBranch}",
                    "push -u"
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
