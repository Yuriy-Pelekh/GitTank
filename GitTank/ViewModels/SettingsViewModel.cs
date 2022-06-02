using GitTank.Loggers;
using GitTank.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using GitTank.Configuration;
using GitTank.CustomCollections;
using Application = System.Windows.Application;

namespace GitTank.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public delegate void ClickEventHandler();

        public event ClickEventHandler Click;
        private readonly GitProcessor _gitProcessor;
        private bool _isAddRepositoryButtonEnabled = true;
        private bool _isRemoveRepositoryButtonEnabled = true;
        private bool _isSaveRepositoriesSettingsButtonEnabled;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public SettingsViewModel(ISettings settings, ILogger logger)
        {
            _gitProcessor = new GitProcessor(settings, logger);
            _logger = logger;
            _settings = settings;
            OnLoaded();
        }

        public bool IsAddRepositoryButtonEnabled
        {
            get => _isAddRepositoryButtonEnabled;
            set
            {
                if (IsAddRepositoryButtonEnabled != value)
                {
                    _isAddRepositoryButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRemoveRepositoryButtonEnabled
        {
            get => _isRemoveRepositoryButtonEnabled;
            set
            {
                if (_isAddRepositoryButtonEnabled != value)
                {
                    _isRemoveRepositoryButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSaveRepositoriesSettingsButtonEnabled
        {
            get => _isSaveRepositoriesSettingsButtonEnabled;
            set
            {
                if (_isSaveRepositoriesSettingsButtonEnabled != value)
                {
                    _isSaveRepositoriesSettingsButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommand _addRepositoryCommand;

        public RelayCommand AddRepositoryCommand
        {
            get { return _addRepositoryCommand ??= new RelayCommand(OpenFolderBrowserDialog); }
        }

        private RelayCommand _removeRepositoryCommand;

        public RelayCommand RemoveRepositoryCommand
        {
            get { return _removeRepositoryCommand ??= new RelayCommand(RemoveSelectedRepository); }
        }

        private RelayCommand _saveRepositoriesSettings;

        public RelayCommand SaveRepositoriesSettingsCommand
        {
            get { return _saveRepositoriesSettings ??= new RelayCommand(SaveRepositoriesSettingToAppConfig); }
        }

        public void SaveRepositoriesSettingToAppConfig()
        {
            OnSetSettingsToAppSettingsFiles();

            var mainWindow = new MainWindow(_settings, _logger);
            mainWindow.Show();
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }

            Click?.Invoke();
        }

        [Obsolete("Move it to Settings class")]
        private void OnSetSettingsToAppSettingsFiles()
        {
#if DEBUG
            var appSettingsPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "appsettings.Development.json");
#else
            var appSettingsPath = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
#endif

            var json = File.ReadAllText(appSettingsPath);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new ExpandoObjectConverter());
            jsonSettings.Converters.Add(new StringEnumConverter());

            dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

            config.appSettings.sources = OnGetAllRepositoriesPathsAndRepo();
            config.appSettings.defaultRepository = SelectedRepository.RepositoryName;
            config.appSettings.defaultBranch = SelectedGitBranch;

            var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

            File.WriteAllText(appSettingsPath, newJson);
        }

        private List<Sources> OnGetAllRepositoriesPathsAndRepo()
        {
            var sources = new List<Sources>();
            var uniqPaths = AllRepositoriesDataCollection
                .Select(item => item.RepositoryPath.Replace($"\\{item.RepositoryName}", string.Empty))
                .Distinct()
                .ToList();

            foreach (var path in uniqPaths)
            {
                var source = new Sources()
                {
                    SourcePath = path,
                    Repositories = new List<string>()
                };
                source.Repositories.AddRange(AllRepositoriesDataCollection
                    .Where(item => item.RepositoryPath.Equals($"{path}\\{item.RepositoryName}"))
                    .Select(item => item.RepositoryName));
                sources.Add(source);
            }

            return sources;
        }

        private void OpenFolderBrowserDialog()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            AddRepositoryToAllSelectedRepositoryList(dialog.SelectedPath);
        }

        private void AddRepositoryToAllSelectedRepositoryList(string repositoryPath)
        {
            var repositories = new Repository();
            try
            {
                var directoryPath = new DirectoryInfo(repositoryPath);
                repositories.RepositoryName = directoryPath.Name;
                repositories.RepositoryPath = directoryPath.FullName;
                repositories.StatusForCheckBox = true;
                if (AllRepositoriesDataCollection.All(path => path.RepositoryPath != repositories.RepositoryPath))
                {
                    AllRepositoriesDataCollection.Add(repositories);
                    UpdateAvailableRepositoriesCollection();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to add repository", ex);
            }
        }

        public void UpdateAvailableRepositoriesCollection()
        {
            foreach (var repository in AllRepositoriesDataCollection)
            {
                if (repository.StatusForCheckBox &&
                    AvailableRepositoriesCollection.All(path => path.RepositoryPath != repository.RepositoryPath))
                {
                    AvailableRepositoriesCollection.Add(repository);
                }

                if (!repository.StatusForCheckBox  &&
                    AvailableRepositoriesCollection.Any(path => path.RepositoryPath == repository.RepositoryPath))
                {
                    AvailableRepositoriesCollection.Remove(repository);
                }
            }
        }

        private Repository _selectedRepositoriesData;

        public Repository SelectedRepositoriesData
        {
            get => _selectedRepositoriesData;
            set
            {
                _selectedRepositoriesData = value;
                OnPropertyChanged();
            }
        }

        public void RemoveSelectedRepository()
        {
            if (SelectedRepositoriesData != null)
            {
                AvailableRepositoriesCollection.Remove(SelectedRepositoriesData);
                AllRepositoriesDataCollection.Remove(SelectedRepositoriesData);
            }
        }

        private void OnLoaded()
        {
            Task.Run(() =>
            {
                var result = ReadSourcesFromConfig();
                UpdateAllRepositories(result);
            });
        }

        private List<Sources> ReadSourcesFromConfig()
        {
            var repositories = _settings.Sources;
            return repositories;
        }

        private void UpdateAllRepositories(List<Sources> sources)
        {
            var defaultRepository = _settings.DefaultRepository;
            var defaultGitBranch = _settings.DefaultBranch;
            foreach (var item in sources)
            {
                foreach (var repo in item.Repositories)
                {
                    var repository = new Repository
                    {
                        RepositoryPath = $"{item.SourcePath}\\{repo}",
                        RepositoryName = repo,
                        StatusForCheckBox = true
                    };

                    if (AllRepositoriesDataCollection.All(path => path.RepositoryPath != repository.RepositoryPath))
                    {
                        AllRepositoriesDataCollection.Add(repository);
                        UpdateAvailableRepositoriesCollection();
                        if (repository.RepositoryName.Equals(defaultRepository))
                        {
                            SelectedRepository = repository;
                        }
                    }
                }
            }

            SelectedGitBranch = defaultGitBranch;
        }

        private DispatcherObservableCollection<Repository> _allRepositoriesDataCollection;

        public DispatcherObservableCollection<Repository> AllRepositoriesDataCollection
        {
            get => _allRepositoriesDataCollection ??= new DispatcherObservableCollection<Repository>();
            set
            {
                if (_allRepositoriesDataCollection == null || !_allRepositoriesDataCollection.Equals(value))
                {
                    _allRepositoriesDataCollection = value;
                    OnPropertyChanged();
                }
            }
        }

        private DispatcherObservableCollection<Repository> _availableRepositoriesCollection;

        public DispatcherObservableCollection<Repository> AvailableRepositoriesCollection
        {
            get => _availableRepositoriesCollection ??= new DispatcherObservableCollection<Repository>();
            set
            {
                if (_allRepositoriesDataCollection == null || !_allRepositoriesDataCollection.Equals(value))
                {
                    _availableRepositoriesCollection = value;
                    OnPropertyChanged();
                }
            }
        }

        private DispatcherObservableCollection<string> _defaultGitBranch;

        public DispatcherObservableCollection<string> DefaultGitBranch
        {
            get => _defaultGitBranch ??= new DispatcherObservableCollection<string>();
            set
            {
                if (_defaultGitBranch != value)
                {
                    _defaultGitBranch = value;
                    OnPropertyChanged();
                }
            }
        }

        private Repository _selectedRepository;

        public Repository SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (_selectedRepository != value)
                {
                    _selectedRepository = value;
                    var repositoryPath = _selectedRepository != null ? SelectedRepository.RepositoryPath : string.Empty;
                    Task.Run(async () => { await UpdateListOfDefaultsGitBranches(repositoryPath); });
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedGitBranch;

        public string SelectedGitBranch
        {
            get => _selectedGitBranch;
            set
            {
                if (_selectedGitBranch != value)
                {
                    _selectedGitBranch = value;
                    OnPropertyChanged();
                    IsSaveRepositoriesSettingsButtonEnabled = true;
                }
            }
        }

        public async Task UpdateListOfDefaultsGitBranches(string repositoryPath)
        {
            if (repositoryPath != string.Empty)
            {
                var branches = await _gitProcessor.GetAllBranches(repositoryPath);
                var gitBranchesNames = new List<string>(branches.Split(Environment.NewLine).ToList());
                DefaultGitBranch.Clear();
                foreach (var branchName in gitBranchesNames)
                {
                    DefaultGitBranch.Add(branchName.Replace("*", string.Empty).Trim());
                }
            }
            else
            {
                DefaultGitBranch.Clear();
                IsSaveRepositoriesSettingsButtonEnabled = false;
            }
        }
    }
}
