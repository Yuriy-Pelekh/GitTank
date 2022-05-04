using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;

namespace GitTank.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly GitProcessor _gitProcessor;

        private string _selectedRepo;
        private string _selectedBranch;
        private bool _areAllGitCommandButtonsEnabled = true;
        private TabWithLogsViewModel _selectedTab;

        public ObservableCollection<string> Repositories { get; set; }
        public ObservableCollection<string> Branches { get; set; }

        public ObservableCollection<TabWithLogsViewModel> TabsWithLogs { get; set; }

        public bool IsNewUI => _configuration.GetValue<bool>("appSettings:newUI");

        #region binding properties
        public string SelectedRepo
        {
            get => _selectedRepo;
            set
            {
                if (_selectedRepo != value)
                {
                    _selectedRepo = value;
                    SelectedTab = TabsWithLogs.FirstOrDefault(t => t.Header == _selectedRepo);
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                if (value != _selectedBranch)
                {
                    _selectedBranch = value;
                    OnPropertyChanged();
                }
            }
        }

        public TabWithLogsViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _newBranchName;
        public string NewBranchName
        {
            get => _newBranchName;
            set
            {
                if (_newBranchName != value)
                {
                    _newBranchName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AreAllGitCommandButtonsEnabled
        {
            get => _areAllGitCommandButtonsEnabled;
            set
            {
                if (_areAllGitCommandButtonsEnabled != value)
                {
                    _areAllGitCommandButtonsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Branch Command

        private RelayCommand _branchCommand;

        public RelayCommand BranchCommand
        {
            get { return _branchCommand ??= new RelayCommand(async () => await Branch()); }
        }

        private async Task<string> Branch()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            var branch = await _gitProcessor.GetBranch();

            AreAllGitCommandButtonsEnabled = true;

            return branch;
        }

        #endregion

        #region Update Command

        private RelayCommand _updateCommand;

        public RelayCommand UpdateCommand
        {
            get { return _updateCommand ??= new RelayCommand(async () => await Update()); }
        }

        private async Task Update()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Update();

            AreAllGitCommandButtonsEnabled = true;
        }

        #endregion

        #region Checkout Command

        private RelayCommand _checkoutCommand;

        public RelayCommand CheckoutCommand
        {
            get { return _checkoutCommand ??= new RelayCommand(async () => await Checkout()); }
        }

        private async Task Checkout()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Checkout(SelectedBranch);

            AreAllGitCommandButtonsEnabled = true;
        }

        #endregion

        #region Sync Command

        private RelayCommand _syncCommand;

        public RelayCommand SyncCommand
        {
            get { return _syncCommand ??= new RelayCommand(async () => await Sync()); }
        }

        private async Task Sync()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Sync();

            AreAllGitCommandButtonsEnabled = true;
        }

        #endregion

        #region Push Command

        private RelayCommand _pushCommand;

        public RelayCommand PushCommand
        {
            get { return _pushCommand ??= new RelayCommand(async () => await Push()); }
        }

        private async Task Push()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Push();

            AreAllGitCommandButtonsEnabled = true;
        }

        #endregion

        #region Fetch Command
        private RelayCommand _fetchCommand;

        public RelayCommand FetchCommand
        {
            get { return _fetchCommand ??= new RelayCommand(async () => await Fetch()); }
        }

        private async Task Fetch()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Fetch();

            AreAllGitCommandButtonsEnabled = true;
        }
        #endregion

        #region Create Branch Command
        private RelayCommand _createBranchCommand;

        public RelayCommand CreateBranchCommand
        {
            get { return _createBranchCommand ??= new RelayCommand(async () => await CreateBranch()); }
        }

        private async Task CreateBranch()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.CreateBranch(_newBranchName);

            AreAllGitCommandButtonsEnabled = true;
        }
        #endregion

        #region OpenTerminal Command

        private RelayCommand _openTerminalCommand;

        public RelayCommand OpenTerminalCommand
        {
            get { return _openTerminalCommand ??= new RelayCommand(async () => await OpenTerminal()); }
        }

        private async Task OpenTerminal()
        {
            await _gitProcessor.OpenTerminal(SelectedRepo);
        }

        #endregion

        #region Settings Comand
        private RelayCommand _settingsCommand;
        private ILogger _logger;

        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ??= new RelayCommand(() => OpenSettings()); }
        }

        private void OpenSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow(_configuration, _logger);
            settingsWindow.Show();
        }
        #endregion

        public MainViewModel(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _gitProcessor = new GitProcessor(configuration, logger);
            _gitProcessor.Output += OnOutput;
            _logger = logger;
            OnLoaded();
        }

        private void OnLoaded()
        {
            Task.Run(() =>
            {
                if (Repositories == null)
                {
                    var defaultRepository = _configuration.GetValue<string>("appSettings:defaultRepository");
                    var repositories = _configuration.GetSection("appSettings:repositories")
                        .GetChildren()
                        .Where(c => c.Value != null)
                        .Select(c => c.Value)
                        .ToList();

                    GenerateTabsForLogs(repositories);

                    Repositories = new ObservableCollection<string>(repositories);
                    SelectedRepo = defaultRepository;
                }
            }).ContinueWith(t =>
            {
                if (Branches == null)
                {
                    var remoteBranches = _gitProcessor.Branches().Result;
                    Branches = new ObservableCollection<string>();
                    foreach (var remoteBranch in remoteBranches.Split(Environment.NewLine))
                    {
                        if (!string.IsNullOrWhiteSpace(remoteBranch.Trim()))
                        {
                            if (remoteBranch.StartsWith("*"))
                            {
                                var currentBranch = remoteBranch.Replace("*", string.Empty).Trim();
                                Branches.Add(currentBranch);
                                SelectedBranch = currentBranch;
                            }
                            else
                            {
                                Branches.Add(remoteBranch.Trim());
                            }
                        }
                    }
                }
            });
        }

        private void GenerateTabsForLogs(List<string> repositories)
        {
            ObservableCollection<TabWithLogsViewModel> tabs = new();

            foreach (var repository in repositories)
            {
                TabWithLogsViewModel tab = new() { Header = repository };
                tabs.Add(tab);
            }

            TabsWithLogs = tabs;
        }

        private void OnOutput(int repositoryIndex, string line)
        {
            TabsWithLogs[repositoryIndex].OutputInfo += line + Environment.NewLine;
        }

        private void ClearGitLogs()
        {
            foreach (var tab in TabsWithLogs)
            {
                tab.OutputInfo = string.Empty;
            }
        }
    }
}
