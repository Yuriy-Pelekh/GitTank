﻿using GitTank.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;
using Serilog.Context;
using System.Diagnostics;
using GitTank.Configuration;
using GitTank.Common;

namespace GitTank.Core
{
    internal class GitProcessor
    {
        public event OutputPerRepositoryEventHandler Output;
        private const string Command = "git";
        private readonly string _defaultRepository;
        private readonly string _defaultBranch;
        private readonly IEnumerable<string> _repositories;
        private readonly List<Sources> _sources;
        private readonly ILogger _logger;

        public GitProcessor(ISettings settings, ILogger logger)
        {
            _logger = logger;

            LogContext.PushProperty(Constants.SourceContext, GetType().Name);

            _defaultRepository = settings.DefaultRepository;
            logger.Debug($"Default repository: {_defaultRepository}");

            _defaultBranch = settings.DefaultBranch;
            logger.Debug($"Default branch: {_defaultBranch}");

            _sources = settings.Sources;

            _repositories = _sources
                .SelectMany(source => source.Repositories)
                .ToList();

            var sources = _sources.Select(source =>
                $"SourcePath: {source.SourcePath}. Repositories: {string.Join(", ", source.Repositories)}{Environment.NewLine}");
            foreach (var source in sources)
            {
                logger.Debug(source);
            }
        }

        private void OnOutput(int repositoryIndex, string line)
        {
            Output?.Invoke(repositoryIndex, line);
        }

        private string GetWorkingDirectoryByRepositoryName(string repositoryName)
        {
            return _sources
                .Where(source => source.Repositories.Contains(repositoryName))
                .Select(source => Path.GetFullPath(source.SourcePath))
                .FirstOrDefault();
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
            //"git branch --show-current";
            const string arguments = "rev-parse --abbrev-ref HEAD";
            Task<string> defaultRepositoryCurrentBranchTask = Task.FromResult(string.Empty);

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(rootWorkingDirectory, repositories[i]);
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
                "fetch -v --progress --prune \"origin\"",
                "pull --progress -v --no-rebase \"origin\"",
                "remote prune origin"
            };

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            List<string> repositories = _repositories.ToList();
            for (var i = 0; i < repositories.Count; i++)
            {
                var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(rootWorkingDirectory, repositories[i]);
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

            var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(_defaultRepository);
            var workingDirectory = Path.Combine(rootWorkingDirectory, _defaultRepository);
            var index = _repositories.ToList().IndexOf(_defaultRepository);
            var processHelper = GetProcessHelper(index, workingDirectory);

            var taskResult = await processHelper.Execute(Command, arguments);

            ReleaseProcessHelperUnmanagedResources(processHelper);

            return taskResult;
        }

        public async Task<string> GetAllBranches(string repositoryPath)
        {
            const string arguments = "branch"; // -r - only remote, -a - all

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
                var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(rootWorkingDirectory, repositories[i]);
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
                var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[i]);
                var workingDirectory = Path.Combine(rootWorkingDirectory, repositories[i]);
                var processHelper = GetProcessHelper(i, workingDirectory);

                runningTasks.Add(processHelper.Execute(Command, arguments));
                processHelpers.Add(processHelper);
            }

            await Task.WhenAll(runningTasks);

            ReleaseProcessHelperUnmanagedResources(processHelpers);
        }

        public async Task OpenTerminal(string selectedRepository)
        {
            var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(selectedRepository);
            var workingDirectory = Path.Combine(rootWorkingDirectory, selectedRepository);

            var terminalStartInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                WorkingDirectory = workingDirectory,
                FileName = @"C:\Program Files\Git\git-bash.exe",
                WindowStyle = ProcessWindowStyle.Normal
            };

            var commandLog = $"Terminal started: {terminalStartInfo.FileName} {terminalStartInfo.Arguments} in {terminalStartInfo.WorkingDirectory}";

            try
            {
                var terminalProcess = Process.Start(terminalStartInfo);
                _logger.Information($"{commandLog}, PID: {terminalProcess?.Id}, Process Name: {terminalProcess?.ProcessName}");
                await terminalProcess?.WaitForExitAsync()!;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open terminal", ex);
            }
        }

        public async Task Fetch()
        {
            const string argument = "fetch -v --progress --prune \"origin\"";

            List<Task> runningTasks = new();
            List<ProcessHelper> processHelpers = new();
            List<string> repositories = _repositories.ToList();
            for (var repositoryIndex = 0; repositoryIndex < repositories.Count; repositoryIndex++)
            {
                var rootWorkingDirectory = GetWorkingDirectoryByRepositoryName(repositories[repositoryIndex]);
                var workingDirectory = Path.Combine(rootWorkingDirectory, repositories[repositoryIndex]);
                var processHelper = GetProcessHelper(repositoryIndex, workingDirectory);

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
