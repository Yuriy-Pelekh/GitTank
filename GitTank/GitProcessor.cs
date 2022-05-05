using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;
using Serilog.Context;
using System.Diagnostics;

namespace GitTank
{
    internal class GitProcessor
    {
        public event OutputPerRepositoryEventHandler Output;
        private const string Command = "git";
        private readonly string _rootWorkingDirectory;
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;

        private readonly ILogger _logger;

        public GitProcessor(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;

            LogContext.PushProperty(Constants.SourceContext, GetType().Name);

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

        private void OnOutput(int repositoryIndex, string line)
        {
            Output?.Invoke(repositoryIndex, line);
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
            for (int i = 0; i < repositories.Count; i++)
            {
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

            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);
            int index = _repositories.ToList().IndexOf(_defaultRepository);
            var processHelper = GetProcessHelper(index, workingDirectory);

            var taskResult = await processHelper.Execute(Command, arguments);

            ReleaseProcessHelperUnmanagedResources(processHelper);

            return taskResult;
        }

        public async Task<string> GetAllBranches(string repositoryPath)
        {
            const string arguments = "branch -a"; // -r - only remote, -a - all

            int index = _repositories.ToList().IndexOf(_defaultRepository);
            var processHelper = GetProcessHelper(index, repositoryPath);

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
            List<string> repositories = _repositories.ToList();
            for (int i = 0; i < repositories.Count; i++)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

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
            List<string> repositories = _repositories.ToList();
            for (int i = 0; i < repositories.Count; i++)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);

                runningTasks.Add(SyncOneRepository(i, workingDirectory));
            }

            await Task.WhenAll(runningTasks);
        }

        private async Task SyncOneRepository(int repositoryIndex, string repositoryDirectory)
        {
            var fetchArgument = "fetch -v --progress --prune \"origin\"";
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
            for (int i = 0; i < repositories.Count; i++)
            {
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
            for (int i = 0; i < repositories.Count; i++)
            {
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
            List<Task> runningTasks = new();
            List<string> repositories = _repositories.ToList();
            for (int i = 0; i < repositories.Count; i++)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repositories[i]);

                runningTasks.Add(CreateBranchOneRepository(newBranch, i, workingDirectory));
            }

            await Task.WhenAll(runningTasks);
        }

        private async Task CreateBranchOneRepository(string newBranch, int repositoryIndex, string repositoryDirectory)
        {
            string pullArgument = "pull --progress -v --no-rebase \"origin\"";
            string checkoutNewBranchArgument = $"checkout -b {newBranch}";
            string pushArgument = $"push -u origin {newBranch}";

            var processHelper = GetProcessHelper(repositoryIndex, repositoryDirectory);

            await processHelper.Execute(Command, pullArgument);
            await processHelper.Execute(Command, checkoutNewBranchArgument);
            await processHelper.Execute(Command, pushArgument);

            ReleaseProcessHelperUnmanagedResources(processHelper);
        }
    }
}
