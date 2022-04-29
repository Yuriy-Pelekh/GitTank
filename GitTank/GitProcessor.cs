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
            Task<string> defaultRepositoryCurrentBranchTask = Task.FromResult(string.Empty);

            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, arguments, workingDirectory);

                Task<string> task = _processHelper.Execute();
                runningTasks.Add(task);
                
                if (string.Equals(repository, _defaultRepository))
                {
                    defaultRepositoryCurrentBranchTask = task;
                }
            }

            await Task.WhenAll(runningTasks);
            return await defaultRepositoryCurrentBranchTask;
        }

        public async Task Update()
        {
            string[] arguments =
            {
                //"fetch -v --progress --prune \"origin\"",
                "pull --progress -v --no-rebase \"origin\"",
                //"remote prune origin"
            };

            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);

                foreach (var argument in arguments)
                {
                    _processHelper.Configure(Command, argument, workingDirectory);
                    runningTasks.Add(_processHelper.Execute());
                }
            }

            await Task.WhenAll(runningTasks);
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
            string arguments;

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
            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);

                runningTasks.Add(SyncOneRepository(workingDirectory));
            }

            await Task.WhenAll(runningTasks);
        }

        private async Task SyncOneRepository(string repositoryDirectory)
        {
            string fetchArgument = "fetch -v --progress --prune \"origin\"";
            string mergeArgument = $"merge origin/{_defaultBranch}";

            _processHelper.Configure(Command, fetchArgument, repositoryDirectory);
            await _processHelper.Execute();

            _processHelper.Configure(Command, mergeArgument, repositoryDirectory);
            await _processHelper.Execute();
        }

        public async Task Push()
        {
            // Requires: git config --global push.default current
            const string arguments = "push -u";

            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                _processHelper.Configure(Command, arguments, workingDirectory);
                runningTasks.Add(_processHelper.Execute());
            }

            await Task.WhenAll(runningTasks);
        }

        public async Task Fetch()
        {
            const string argument = "fetch -v --progress --prune \"origin\"";

            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);                

                _processHelper.Configure(Command, argument, workingDirectory);
                runningTasks.Add(_processHelper.Execute());
            }

            await Task.WhenAll(runningTasks);
        }

        public async Task CreateBranch(string currentBranch, string newBranch)
        {
            List<Task> runningTasks = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);

                runningTasks.Add(CreateBranchOneRepository(currentBranch, newBranch, workingDirectory));
            }

            await Task.WhenAll(runningTasks);
        }

        private async Task CreateBranchOneRepository(string currentBranch, string newBranch, string repositoryDirectory)
        {
            string checkoutCurrentBranchArgument = $"checkout {currentBranch}";
            string pullArgument = "pull --progress -v --no-rebase \"origin\"";
            string checkoutNewBranchArgument = $"checkout -b {newBranch}";
            string pushArgument = "push -u";

            _processHelper.Configure(Command, checkoutCurrentBranchArgument, repositoryDirectory);
            await _processHelper.Execute();

            _processHelper.Configure(Command, pullArgument, repositoryDirectory);
            await _processHelper.Execute();

            _processHelper.Configure(Command, checkoutNewBranchArgument, repositoryDirectory);
            await _processHelper.Execute();

            _processHelper.Configure(Command, pushArgument, repositoryDirectory);
            await _processHelper.Execute();
        }
    }
}
