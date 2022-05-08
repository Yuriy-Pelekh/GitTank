using GitTank.Dto;
using GitTank.Helpers;
using GitTank.Loggers;
using GitTank.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace GitTank.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public delegate void OnClickEventHandler();
        public event OnClickEventHandler OnClick;
        private readonly GitProcessor _gitProcessor;
        private bool _isAddRepositoryButtonEnabled = true;
        private bool _isRemoveRepositoryButtonEnabled = true;
        private bool _isSaveRepositoriesSettingsButtonEnabled = true;
        private IConfiguration _configuration;
        private ILogger _logger;

        public SettingsViewModel(IConfiguration configuration, ILogger logger)
        {
            _gitProcessor = new GitProcessor(configuration, logger);
            _logger = logger;
            _configuration = configuration;
            OnLoaded();
        }

        public bool IsAddRepositoryButtonEnabled
        {
            get => _isAddRepositoryButtonEnabled;
            set
            {
                if(IsAddRepositoryButtonEnabled != value)
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
                if(_isAddRepositoryButtonEnabled!= value)
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
            setSettingsToAppsettingsFiles();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName)
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            MainWindow mainWindow = new MainWindow(_configuration, _logger);
            mainWindow.Show();
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
            OnClick?.Invoke();
        }

        private void setSettingsToAppsettingsFiles()
        {
            var appSettingsPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "appsettings.Development.json");
            var json = File.ReadAllText(appSettingsPath);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new ExpandoObjectConverter());
            jsonSettings.Converters.Add(new StringEnumConverter());

            dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

            config.appSettings.sources = getAllRepositoriesPathesAndRepo();
            config.appSettings.defaultRepository = SelectedRepository.RepositoryName;
            config.appSettings.defaultBranch = SelectedGitBranch;

            var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

            File.WriteAllText(appSettingsPath, newJson);
        }

        private List<Sources> getAllRepositoriesPathesAndRepo()
        {
            List<Sources> sources = new List<Sources>();
            List<string> uniqPathes = (AllRepositoriesDataCollection.Select(item => item.RepositoryPath.Replace($"\\{item.RepositoryName}", ""))).Distinct().ToList();
            foreach (var path in uniqPathes)
            {
                Sources source = new Sources()
                {
                    sourcePath = path,
                    repositories = new List<string>()
                };
                source.repositories.AddRange(AllRepositoriesDataCollection.Where(item => item.RepositoryPath.Contains(path)).Select(item => item.RepositoryName));
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
            catch(Exception ex)
            {
                _logger.Error("Failed to add repository",ex);
            }

        }

        public void UpdateAvailableRepositoriesCollection()
        {
            foreach (var repository in AllRepositoriesDataCollection)
            {
                if (repository.StatusForCheckBox && AvailableRepositoriesCollection.All(path => path.RepositoryPath != repository.RepositoryPath))
                {
                    AvailableRepositoriesCollection.Add(repository);
                }
                if (repository.StatusForCheckBox == false && AvailableRepositoriesCollection.Any(path => path.RepositoryPath == repository.RepositoryPath))
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
            var repositories = _configuration.GetSection("appSettings").GetSection("sources").Get<List<Sources>>()
                                .ToList();
            return repositories;
        }

        private void UpdateAllRepositories(List<Sources> sources)
        {
            var defaultRepository = _configuration.GetValue<string>("appSettings:defaultRepository");
            var defaultGitBranch = _configuration.GetValue<string>("appSettings:defaultBranch");
            foreach (var item in sources)
            {
                foreach(var repo in item.repositories)
                {
                    var repository = new Repository();
                    repository.RepositoryPath = $"{item.sourcePath}\\{repo}";
                    repository.RepositoryName = repo;
                    repository.StatusForCheckBox = true;
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
                _defaultGitBranch = value;
                OnPropertyChanged();
            }
        }

        private Repository _selectedRepository;
        public Repository SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                _selectedRepository = value;
                Task.Run(async () => { await UpdateListOfDefaultsGitBranches(SelectedRepository.RepositoryPath); });              
            }
        }

        public string SelectedGitBranch { get; set; }

        public async Task UpdateListOfDefaultsGitBranches(string repositoryPath)
        {
                var branches = await _gitProcessor.GetAllBranches(repositoryPath);
                var gitBranchesNames = new List<string>(branches.Split("\r\n").ToList());
                DefaultGitBranch.Clear();
                foreach (var branchName in gitBranchesNames)
                {
                    DefaultGitBranch.Add(branchName.Replace("*", string.Empty).Trim());
                }
        }
    }
}
