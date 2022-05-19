using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GitTank.Loggers;
using System.Collections.Generic;
using System.Windows;
using GitTank.CustomCollections;
using GitTank.Models;

namespace GitTank.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly GitProcessor _gitProcessor;

        private string _selectedRepository;
        private string _selectedBranch;
        private bool _areAllGitCommandButtonsEnabled = true;
        private TabWithLogsViewModel _selectedTab;

        public ObservableCollection<string> Repositories { get; set; } = new();
        public DispatcherObservableCollection<string> Branches { get; set; } = new();

        public ObservableCollection<TabWithLogsViewModel> TabsWithLogs { get; set; }

        public bool IsNewUI => _configuration.GetValue<bool>("appSettings:newUI");

        public string SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (_selectedRepository != value)
                {
                    _selectedRepository = value;
                    SelectedTab = TabsWithLogs.FirstOrDefault(t => t.Header == _selectedRepository);
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

        #region Branch Command

        private RelayCommand _branchCommand;

        public RelayCommand BranchCommand
        {
            get
            {
                async void Execute() => await Branch();
                return _branchCommand ??= new RelayCommand(Execute);
            }
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
            get
            {
                async void Execute() => await Update();
                return _updateCommand ??= new RelayCommand(Execute);
            }
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
            get
            {
                async void Execute() => await Checkout();
                return _checkoutCommand ??= new RelayCommand(Execute);
            }
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
            get
            {
                async void Execute() => await Sync();
                return _syncCommand ??= new RelayCommand(Execute);
            }
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
            get
            {
                async void Execute() => await Push();
                return _pushCommand ??= new RelayCommand(Execute);
            }
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
            get
            {
                async void Execute() => await Fetch();
                return _fetchCommand ??= new RelayCommand(Execute);
            }
        }

        private async Task Fetch()
        {
            AreAllGitCommandButtonsEnabled = false;
            ClearGitLogs();

            await _gitProcessor.Fetch();

            AreAllGitCommandButtonsEnabled = true;
        }
        #endregion

        #region OpenTerminal Command

        private RelayCommand _openTerminalCommand;

        public RelayCommand OpenTerminalCommand
        {
            get
            {
                async void Execute() => await OpenTerminal();
                return _openTerminalCommand ??= new RelayCommand(Execute);
            }
        }

        private async Task OpenTerminal()
        {
            await _gitProcessor.OpenTerminal(SelectedRepository);
        }

        #endregion

        #region Settings Command
        private RelayCommand _settingsCommand;
        private readonly ILogger _logger;

        public RelayCommand SettingsCommand
        {
            get { return _settingsCommand ??= new RelayCommand(() => OpenSettings()); }
        }

        private void OpenSettings()
        {
            SettingsWindow settingsWindow = new(_configuration, _logger);
            settingsWindow.Show();
        }
        #endregion

        private RelayCommand _openCreateBranchWindowCommand;
        private bool _showShadow;

        public RelayCommand OpenCreateBranchWindowCommand
        {
            get { return _openCreateBranchWindowCommand ??= new RelayCommand(OpenCreateBranchWindow); }
        }

        public bool ShowShadow
        {
            get => _showShadow;
            set
            {
                if (_showShadow != value)
                {
                    _showShadow = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OpenCreateBranchWindow()
        {
            CreateBranchWindow createBranchWindow = new(_configuration, _logger);
            createBranchWindow.Closing += OnCreateBranchWindowClosing;
            ShowShadow = true;
            createBranchWindow.ShowDialog();
        }

        private void OnCreateBranchWindowClosing(object sender, EventArgs e)
        {
            ((CreateBranchWindow)sender).Closing -= OnCreateBranchWindowClosing;
            ShowShadow = false;
            Application.Current.Dispatcher.Invoke(async () => { await UpdateBranches(); });
        }

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
                var result = ReadReposFromConfig();
                GenerateTabsForLogs(result);
                UpdateRepositories(result);
            }).ContinueWith(async _ => { await UpdateBranches(); });
        }

        private async Task UpdateBranches()
        {
            var remoteBranches = await _gitProcessor.Branches();
            Branches?.Clear();
            foreach (var remoteBranch in remoteBranches.Split(Environment.NewLine))
            {
                if (!string.IsNullOrWhiteSpace(remoteBranch.Trim()))
                {
                    if (remoteBranch.StartsWith("*"))
                    {
                        var currentBranch = remoteBranch.Replace("*", string.Empty).Trim();
                        Branches?.Add(currentBranch);
                        SelectedBranch = currentBranch;
                    }
                    else
                    {
                        Branches?.Add(remoteBranch.Trim());
                    }
                }
            }
        }

        private void UpdateRepositories(List<string> repositories)
        {
            var defaultRepository = _configuration.GetValue<string>("appSettings:defaultRepository");
            Repositories.Clear();
            Repositories = new ObservableCollection<string>(repositories);
            SelectedRepository = defaultRepository;
        }

        private List<string> ReadReposFromConfig()
        {
            var repositories = _configuration
                .GetSection("appSettings")
                .GetSection("sources")
                .Get<List<Sources>>()
                .SelectMany(c => c.Repositories)
                .ToList();
            return repositories;
        }

        private void GenerateTabsForLogs(IEnumerable<string> repositories)
        {
            ObservableCollection<TabWithLogsViewModel> tabs = new(repositories.Select(repositoryName => new TabWithLogsViewModel { Header = repositoryName }));
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
