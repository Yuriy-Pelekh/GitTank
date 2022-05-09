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
        public event OutputPerRepositoryEventHandler Output;
        private const string Command = "git";
        private string _rootWorkingDirectory;
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;
        private List<Sources> _sources;
        private readonly ILogger _logger;

        public GitProcessor(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;

            LogContext.PushProperty(Constants.SourceContext, GetType().Name);

            _defaultRepository = configuration.GetValue<string>("appSettings:defaultRepository");
            logger.Debug($"Default repository: {_defaultRepository}");

            _defaultBranch = configuration.GetValue<string>("appSettings:defaultBranch");
            logger.Debug($"Default branch: {_defaultBranch}");

            _sources = configuration.GetSection("appSettings").GetSection("sources").Get<List<Sources>>();

            _repositories = _sources.SelectMany(c => c.repositories).ToList();
            logger.Debug($"Repositories: {string.Join(", ", _repositories)}");
        }

        private void OnOutput(int repositoryIndex, string line)
        {
            Output?.Invoke(repositoryIndex, line);
        }

        private string GetWorkingDirectoryByRepositoryName(string repositoryName)
        {
            string workingDirectory = null;
            foreach (var source in _sources.Where(source => source.repositories.Contains(repositoryName)))
            {
                workingDirectory = Path.GetFullPath(source.sourcePath);
                break;
            }

            return workingDirectory;
        }

        private ProcessHelper GetProcessHelper(int repositoryIndex, string workingDirectory)
        {
            var processHelper = new ProcessHelper(_logger, workingDirectory, repositoryIndex);
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
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

                Task<string> task = processHelper.Execute(Command, arguments);
                runningTasks.Add(task);
                processHelpers.Add(processHelper);

                if (string.Equals(repositories[i], _defaultRepository))
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
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

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

            _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(_defaultRepository);
            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);
            var index = _repositories.ToList().IndexOf(_defaultRepository);
            var processHelper = GetProcessHelper(index, workingDirectory);

            var taskResult = await processHelper.Execute(Command, arguments);

            ReleaseProcessHelperUnmanagedResources(processHelper);

            return taskResult;
        }

        public async Task<string> GetAllBranches(string repositoryPath)
        {
            const string arguments = "branch -a"; // -r - only remote, -a - all

            var index = _repositories.ToList().IndexOf(_defaultRepository);
            var processHelper = GetProcessHelper(index, repositoryPath);

            var taskResult = await processHelper.Execute(Command, arguments);

            ReleaseProcessHelperUnmanagedResources(processHelper);

            return taskResult;
        }

        public async Task Checkout(string selectedItem)
        {
            const string remoteBranchExistsCommand = "ls-remote --heads origin {0}";
            const string localBranchExistsCommand = "branch --list {0}";

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

                var localBranchExists = await processHelper.Execute(Command, string.Format(localBranchExistsCommand, selectedItem));
                var remoteBranchExists = await processHelper.Execute(Command, string.Format(remoteBranchExistsCommand, selectedItem));

                var arguments = !string.IsNullOrWhiteSpace(localBranchExists) || !string.IsNullOrWhiteSpace(remoteBranchExists)
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
            List<string> repositories = _repositories.ToList();
            List<Task> runningTasks = repositories
                .Select(repository => Path.Combine(GetWorkingDirectoryByRepositoryName(repository), repository))
                .Select((repositoryPath, repositoryIndex) => SyncOneRepository(repositoryIndex, repositoryPath))
                .ToList();

            await Task.WhenAll(runningTasks);
        }

        private async Task SyncOneRepository(int repositoryIndex, string repositoryDirectory)
        {
            const string fetchArgument = "fetch -v --progress --prune \"origin\"";
            var mergeArgument = $"merge origin/{_defaultBranch}";

            var processHelper = GetProcessHelper(repositoryIndex, repositoryDirectory);

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
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

                runningTasks.Add(processHelper.Execute(Command, arguments));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
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
            const string argument = "fetch -v --progress --prune \"origin\"";

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                _rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

                runningTasks.Add(processHelper.Execute(Command, argument));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
        }

        public async Task CreateBranch(string newBranch)
        {
            List<string> repositories = _repositories.ToList();
            List<Task> runningTasks = repositories
                .Select(repository => Path.Combine(GetWorkingDirectoryByRepositoryName(repository), repository))
                .Select((repositoryPath, repositoryIndex) => CreateBranchOneRepository(newBranch, repositoryIndex, repositoryPath))
                .ToList();

            await Task.WhenAll(runningTasks);
        }

        private async Task CreateBranchOneRepository(string newBranch, int repositoryIndex, string repositoryDirectory)
        {
            const string pullArgument = "pull --progress -v --no-rebase \"origin\"";
            var checkoutNewBranchArgument = $"checkout -b {newBranch}";
            var pushArgument = $"push -u origin {newBranch}";

            var processHelper = GetProcessHelper(repositoryIndex, repositoryDirectory);

            await processHelper.Execute(Command, pullArgument);
            await processHelper.Execute(Command, checkoutNewBranchArgument);
            await processHelper.Execute(Command, pushArgument);

            ReleaseProcessHelperUnmanagedResources(processHelper);
        }
    }
}
