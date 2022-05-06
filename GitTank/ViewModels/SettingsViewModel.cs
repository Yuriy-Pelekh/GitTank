using GitTank.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitTank.Models;

namespace GitTank.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly GitProcessor _gitProcessor;
        private bool _isAddRepositoryButtonEnabled = true;
        private bool _isRemoveRepositoryButtonEnabled = true;
        private bool _isSaveRepositoriesSettingsButtonEnabled = true;

        public SettingsViewModel(IConfiguration configuration, ILogger logger)
        {
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
            var branches = _gitProcessor.GetAllBranches(repositoryPath).Result;
            var gitBranchesNames = new List<string>(branches.Split("\r\n").ToList());
            foreach (var branchName in gitBranchesNames)
            {
                DefaultGitBranch.Add(branchName.Replace("*", string.Empty).Trim());
            }
        }
    }
}
