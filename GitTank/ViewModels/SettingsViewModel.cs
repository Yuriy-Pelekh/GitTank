using GitTank.Dto;
using GitTank.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GitTank.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly GitProcessor _gitProcessor;
        private bool _isAddRepositoryButtonEnable = true;
        private bool _isRemoveRepositorieButtonEnabled = true;
        private bool _isSaveRepositoriesSettingsButtonEnabled = true;

        public SettingsViewModel(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _gitProcessor = new GitProcessor(_configuration, logger);
        }

        public bool IsAddRepositoryButtonEnable
        {
            get => _isAddRepositoryButtonEnable; 
            set
            {
                if(IsAddRepositoryButtonEnable != value)
                {
                    _isAddRepositoryButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRemoveRepositorieButtonEnabled
        {
            get => _isRemoveRepositorieButtonEnabled;
            set
            {
                if(IsAddRepositoryButtonEnable!= value)
                {
                    _isRemoveRepositorieButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSaveRepositoriesSettingsButtonEnabled
        {
            get => _isSaveRepositoriesSettingsButtonEnabled;
            set
            {
                if (IsAddRepositoryButtonEnable != value)
                {
                    _isSaveRepositoriesSettingsButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommand _addRepositoryCommand;

        public RelayCommand AddRepositoryCommand
        {
            get { return _addRepositoryCommand ??= new RelayCommand(() => OpenFolderBrowserDialog()); }
        }

        private RelayCommand _removeRepositoryCommand;

        public RelayCommand RemoveRepositoryCommand
        {
            get { return _removeRepositoryCommand ??= new RelayCommand(() => RemoveSelectedRepository()); }
        }

        private RelayCommand _saveRepositoriesSettings;
        public RelayCommand SaveRepositoriesSettingsCommand
        {
            get { return _saveRepositoriesSettings ??= new RelayCommand(() => SaveRepositoriesSettingToAppConfig()); }
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
            var repositories = new RepositoryDto();
            DirectoryInfo directoryPath = new DirectoryInfo(repositoryPath);
            repositories.RepositoryName = directoryPath.Name;
            repositories.RepositoryPath = directoryPath.FullName;
            repositories.StatusForCheckBox = true;
            if (!AllRepositoriesDataCollection.Any(path => path.RepositoryPath == repositories.RepositoryPath))
            {
                AllRepositoriesDataCollection.Add(repositories);
                UpadateAvailableRepositoriesCollection();
            }          
        }

        public void UpadateAvailableRepositoriesCollection()
        {
            foreach (RepositoryDto repository in AllRepositoriesDataCollection)
            {
                if (repository.StatusForCheckBox == true && !AvaliableRepositoriesCollection.Any(path => path.RepositoryPath == repository.RepositoryPath))
                {
                    AvaliableRepositoriesCollection.Add(repository);
                }
                if (repository.StatusForCheckBox == false && AvaliableRepositoriesCollection.Any(path => path.RepositoryPath == repository.RepositoryPath))
                {
                    AvaliableRepositoriesCollection.Remove(repository);
                }
            }
        }

        RepositoryDto _selectedRepositoiesData;
        public RepositoryDto SelectedRepositoiesData
        {
            get => _selectedRepositoiesData;
            set
            {
                _selectedRepositoiesData = value;
                OnPropertyChanged();
            }
        }

        public void RemoveSelectedRepository()
        {
            if (SelectedRepositoiesData != null)
            {
                AvaliableRepositoriesCollection.Remove(SelectedRepositoiesData);
                AllRepositoriesDataCollection.Remove(SelectedRepositoiesData);
            }         
        }


        private ObservableCollection<RepositoryDto> _allRepositoriesDataCollection;

        public ObservableCollection<RepositoryDto> AllRepositoriesDataCollection
        {
            get
            {
                if (_allRepositoriesDataCollection == null)
                {
                    _allRepositoriesDataCollection = new ObservableCollection<RepositoryDto>();
                }
                return _allRepositoriesDataCollection;
            }
            set
            {
                _allRepositoriesDataCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RepositoryDto> _avaliableRepositoriesCollection;
        public ObservableCollection<RepositoryDto> AvaliableRepositoriesCollection
        {
            get
            {
                if (_avaliableRepositoriesCollection == null)
                {
                    _avaliableRepositoriesCollection = new ObservableCollection<RepositoryDto>();
                }
                return _avaliableRepositoriesCollection;
            }
            set
            {
                _avaliableRepositoriesCollection = value;
                OnPropertyChanged();
            }
        }

       private ObservableCollection<string> _defaultGitBranch;
       public ObservableCollection<string> DefaultGitBranch
        {
            get
            {
                if (_defaultGitBranch == null)
                {
                    _defaultGitBranch = new ObservableCollection<string>();
                }
                return _defaultGitBranch;
            }
            set
            {
                _defaultGitBranch = value;
                OnPropertyChanged();
            }
        }

        private RepositoryDto _selectedRepository;
        public RepositoryDto SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                _selectedRepository = value;
                UpdateListOfDefaultsGitBranches(SelectedRepository.RepositoryPath);
            }
        }

        private string _selectedGitBranch;
        public string SelectedGitBranch
        {
            get => _selectedGitBranch;
            set
            {
                _selectedGitBranch = value;
            }
        }

        public void UpdateListOfDefaultsGitBranches(string repositoryPath)
        {
            DefaultGitBranch.Clear();
            var branches = _gitProcessor.GetAllBranches(repositoryPath);
            List<string> gitBranchesNames = new List<string>(branches.Split("\r\n").ToList());
            foreach (string branchName in gitBranchesNames)
            {
                DefaultGitBranch.Add(branchName.Replace("*", "").Trim());
            }
        }
    }
}
