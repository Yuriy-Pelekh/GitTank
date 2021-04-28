using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly string _alternativeBranch;

        private List<string> Repositories = new();

        public GitProcessor(IConfiguration configuration)
        {
            _processHelper = new ProcessHelper();
            _processHelper.Output += OnOutput;

            _rootWorkingDirectory = configuration.GetValue<string>("appSettings:sourcePath");
            _defaultRepository = configuration.GetValue<string>("appSettings:defaultRepository");
            _defaultBranch = configuration.GetValue<string>("appSettings:defaultBranch");
            _alternativeBranch = configuration.GetValue<string>("appSettings:alternativeBranch");
            Repositories = configuration.GetSection("appSettings:repositories")
                .GetChildren()
                .Select(c => c.Value)
                .ToList();
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
            var workingDirectory = Path.Combine(_rootWorkingDirectory, _defaultRepository);

            _processHelper.Configure(Command, arguments, workingDirectory);
            return await _processHelper.Execute();
        }

        public async Task Update()
        {
            foreach (var repository in Repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                string[] arguments =
                {
                    "fetch -v --progress --prune \"origin\"",
                    "pull --progress -v --no-rebase \"origin\"",
                    "remote prune origin"
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
            var arguments = $"checkout {selectedItem}";

            foreach (var repository in Repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);

                _processHelper.Configure(Command, arguments, workingDirectory);
                await _processHelper.Execute();
            }
        }

        public async Task Sync()
        {
            const string defaultArguments = "merge origin/";

            foreach (var repository in Repositories)
            {
                var workingDirectory = Path.Combine(_rootWorkingDirectory, repository);
                var arguments = defaultArguments;
                if (repository.Equals(_defaultRepository, StringComparison.OrdinalIgnoreCase))
                {
                    arguments += _alternativeBranch;
                }
                else
                {
                    arguments += _defaultBranch;
                }

                _processHelper.Configure(Command, arguments, workingDirectory);
                await _processHelper.Execute();
            }
        }
    }
}
