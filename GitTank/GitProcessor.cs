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
        private const string Command = "git";
        private readonly string _rootWorkingDirectory;
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;

        private ILogger _logger;

        public GitProcessor(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;

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

        private void OnOutput(string line)
        {
            Output?.Invoke(line);
        }

        private ProcessHelper GetProcessHelper(string workingDirectory)
        {
            var processHelper = new ProcessHelper(_logger, workingDirectory);
            processHelper.Output += OnOutput;

            return processHelper;
        }

        private void ReleaseProcessHelperUnmanagedResources(List<ProcessHelper> processHelpers)
        {
            foreach (var processHelper in processHelpers)
            {
                ReleaseProcessHelperUnmanagedResources(processHelper);
            }
        }

        private void ReleaseProcessHelperUnmanagedResources(ProcessHelper processHelper)
        {
            processHelper.Output -= OnOutput;
        }

        public async Task<string> GetBranch()
        {
            const string arguments = "rev-parse --abbrev-ref HEAD"; //"git branch --show-current";
            Task<string> defaultRepositoryCurrentBranchTask = Task.FromResult(string.Empty);

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var processHelper = GetProcessHelper(workingDirectory);

                Task<string> task = processHelper.Execute(Command, arguments);
                runningTasks.Add(task);
                processHelpers.Add(processHelper);

                if (string.Equals(repository, _defaultRepository))
                {
                    defaultRepositoryCurrentBranchTask = task;
                }
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);

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
            List<ProcessHelper> processHelpers = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var processHelper = GetProcessHelper(workingDirectory);

                foreach (var argument in arguments)
                {
                    runningTasks.Add(processHelper.Execute(Command, argument));
                    processHelpers.Add(processHelper);
                }
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
        }

        public async Task<string> Branches()
        {
            const string arguments = "branch"; // -r - only remote, -a - all
            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);
            var processHelper = GetProcessHelper(workingDirectory);

            var taskResult = await processHelper.Execute(Command, arguments);

            ReleaseProcessHelperUnmanagedResources(processHelper);

            return taskResult;
        }

        public async Task Checkout(string selectedItem)
        {
            var remoteBranchExistsCommand = "ls-remote --heads origin {0}";
            var localBranchExistsCommand = "branch --list {0}";
            string arguments;

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var processHelper = GetProcessHelper(workingDirectory);

                var localBranchExists = await processHelper.Execute(Command, string.Format(localBranchExistsCommand, selectedItem));
                var remoteBranchExists = await processHelper.Execute(Command, string.Format(remoteBranchExistsCommand, selectedItem));

                arguments = !string.IsNullOrWhiteSpace(localBranchExists) || !string.IsNullOrWhiteSpace(remoteBranchExists)
                    ? $"checkout {selectedItem}"
                    : $"checkout -b {selectedItem}";

                runningTasks.Add(processHelper.Execute(Command, arguments));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
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

            var processHelper = GetProcessHelper(repositoryDirectory);

            await processHelper.Execute(Command, fetchArgument);
            await processHelper.Execute(Command, mergeArgument);

            ReleaseProcessHelperUnmanagedResources(processHelper);
        }

        public async Task Push()
        {
            // Requires: git config --global push.default current
            const string arguments = "push -u";

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var processHelper = GetProcessHelper(workingDirectory);

                runningTasks.Add(processHelper.Execute(Command, arguments));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
        }

        public async Task Fetch()
        {
            const string argument = "fetch -v --progress --prune \"origin\"";

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            foreach (var repository in _repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var processHelper = GetProcessHelper(workingDirectory);

                runningTasks.Add(processHelper.Execute(Command, argument));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
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

            var processHelper = GetProcessHelper(repositoryDirectory);

            await processHelper.Execute(Command, checkoutCurrentBranchArgument);
            await processHelper.Execute(Command, pullArgument);
            await processHelper.Execute(Command, checkoutNewBranchArgument);
            await processHelper.Execute(Command, pushArgument);

            ReleaseProcessHelperUnmanagedResources(processHelper);
        }
    }
}
