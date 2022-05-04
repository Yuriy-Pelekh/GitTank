using GitTank.Dto;
using GitTank.Loggers;
using GitTank.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly GitProcessor _gitProcessor;
        private bool _isAddRepositoryButtonEnabled = true;
        private bool _isRemoveRepositoryButtonEnabled = true;
        private bool _isSaveRepositoriesSettingsButtonEnabled = true;
        private IConfiguration _configuration;
        private ILogger _logger;

        public SettingsViewModel(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;
            _gitProcessor = new GitProcessor(configuration, logger);
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            openMainWindow(_configuration, _logger);
            this.OnClosingRequest();
        }

        private void setSettingsToAppsettingsFiles()
        {
            var appSettingsPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "appsettings.json");
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

        private void openMainWindow(IConfiguration configuration, ILogger logger)
        {
            
            MainWindow mainWindow = new MainWindow(configuration, logger);
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
            Thread.Sleep(100);
            mainWindow.Show();
        }

        private List<Sources> getAllRepositoriesPathesAndRepo()
        {
            List<Sources> sources = new List<Sources>();
            List<string> uniqPathes = (AllRepositoriesDataCollection.Select(item => item.RepositoryPath.Replace($"\\{item.RepositoryName}", ""))).Distinct().ToList();
            foreach (var path in uniqPathes)
            {
                Sources source = new Sources()
                {
                    SourcePath = path,
                    Repositories = new List<string>()
                };
                source.Repositories.AddRange(AllRepositoriesDataCollection.Where(item => item.RepositoryPath.Contains(path)).Select(item => item.RepositoryName));
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


        private ObservableCollection<Repository> _allRepositoriesDataCollection;

        public ObservableCollection<Repository> AllRepositoriesDataCollection
        {
            get => _allRepositoriesDataCollection ??= new ObservableCollection<Repository>();
            set
            {
                if (_allRepositoriesDataCollection == null || !_allRepositoriesDataCollection.Equals(value))
                {
                    _allRepositoriesDataCollection = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Repository> _availableRepositoriesCollection;
        public ObservableCollection<Repository> AvailableRepositoriesCollection
        {
            get => _availableRepositoriesCollection ??= new ObservableCollection<Repository>();
            set
            {
                if (_allRepositoriesDataCollection == null || !_allRepositoriesDataCollection.Equals(value))
                {
                    _availableRepositoriesCollection = value;
                    OnPropertyChanged();
                }
            }
        }

       private ObservableCollection<string> _defaultGitBranch;
       public ObservableCollection<string> DefaultGitBranch
        {
            get => _defaultGitBranch ??= new ObservableCollection<string>();
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
                UpdateListOfDefaultsGitBranches(SelectedRepository.RepositoryPath);
            }
        }

        public string SelectedGitBranch { get; set; }

        public void UpdateListOfDefaultsGitBranches(string repositoryPath)
        {
            DefaultGitBranch.Clear();
            var branches = _gitProcessor.GetAllBranches(repositoryPath).Result.ToString();
            var gitBranchesNames = new List<string>(branches.Split("\r\n").ToList());
            foreach (var branchName in gitBranchesNames)
            {
                DefaultGitBranch.Add(branchName.Replace("*", string.Empty).Trim());
            }
        }
    }
}
