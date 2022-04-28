using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System;
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
        private string _selectedRepoIndex;
        private string _selectedBranchIndex;
        private string _outputInfo;
        private bool _isUpdateButtonEnable = true;
        private bool _isBranchButtonEnable = true;
        private bool _isCheckoutButtonEnable = true;
        private bool _isSyncButtonEnable = true;
        private bool _isPushButtonEnable = true;
        private bool _isFetchButtonEnable = true;
        private bool _isCreateButtonEnable = true;

        public ObservableCollection<string> Repositories { get; set; }
        public ObservableCollection<string> Branches { get; set; }

        public bool IsNewUI => _configuration.GetValue<bool>("appSettings:newUI");

        public string SelectedRepoIndex
        {
            get => _selectedRepoIndex;
            set
            {
                if (_selectedRepoIndex != value)
                {
                    _selectedRepoIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedBranchIndex
        {
            get => _selectedBranchIndex;
            set
            {
                if (value != _selectedBranchIndex)
                {
                    _selectedBranchIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OutputInfo
        {
            get => _outputInfo;
            set
            {
                if (value is not null)
                {
                    _outputInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUpdateButtonEnable
        {
            get => _isUpdateButtonEnable;
            set
            {
                if (_isUpdateButtonEnable != value)
                {
                    _isUpdateButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBranchButtonEnable
        {
            get => _isBranchButtonEnable;
            set
            {
                if (_isBranchButtonEnable != value)
                {
                    _isBranchButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCheckoutButtonEnable
        {
            get => _isCheckoutButtonEnable;
            set
            {
                if (_isCheckoutButtonEnable != value)
                {
                    _isCheckoutButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSyncButtonEnable
        {
            get => _isSyncButtonEnable;
            set
            {
                if (_isSyncButtonEnable != value)
                {
                    _isSyncButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPushButtonEnable
        {
            get => _isPushButtonEnable;
            set
            {
                if (_isPushButtonEnable != value)
                {
                    _isPushButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _gitProcessor = new GitProcessor(configuration, logger);
            _gitProcessor.Output += OnOutput;
            OnLoaded();
        }

        public bool IsFetchButtonEnable
        {
            get => _isFetchButtonEnable;
            set
            {
                if (_isFetchButtonEnable != value)
                {
                    _isFetchButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCreateButtonEnable
        {
            get => _isCreateButtonEnable;
            set
            {
                if (_isCreateButtonEnable != value)
                {
                    _isCreateButtonEnable = value;
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

        #region Branch Command

        private RelayCommand _branchCommand;

        public RelayCommand BranchCommand
        {
            get { return _branchCommand ??= new RelayCommand(Branch); }
        }

        private void Branch()
        {
            IsBranchButtonEnable = false;
            OutputInfo = string.Empty;
            Task.Run(() =>
            {
                var currentBranch = _gitProcessor.GetBranch();
                IsBranchButtonEnable = true;
            });
        }

        #endregion

        #region Update Command

        private RelayCommand _updateCommand;

        public RelayCommand UpdateCommand
        {
            get { return _updateCommand ??= new RelayCommand(() => Update()); }

        }

        private void Update()
        {
            IsUpdateButtonEnable = false;
            OutputInfo = string.Empty;
            Task.Run(() =>
            {
                var output = _gitProcessor.Update();
                IsUpdateButtonEnable = true;
            });
        }

        #endregion

        #region Checkout Command

        private RelayCommand _checkoutCommand;

        public RelayCommand CheckoutCommand
        {
            get { return _checkoutCommand ??= new RelayCommand(Checkout); }

        }

        private void Checkout()
        {
            IsCheckoutButtonEnable = false;
            OutputInfo = string.Empty;
            var selectedItem = Branches[int.Parse(SelectedBranchIndex)];
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Checkout(selectedItem);
                IsCheckoutButtonEnable = true;
            });
        }

        #endregion

        #region Sync Command

        private RelayCommand _syncCommand;

        public RelayCommand SyncCommand
        {
            get { return _syncCommand ??= new RelayCommand(Sync); }
        }

        private void Sync()
        {
            IsSyncButtonEnable = false;
            OutputInfo = string.Empty;
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Sync();
                IsSyncButtonEnable = true;
            });
        }

        #endregion

        #region Push Command

        private RelayCommand _pushCommand;

        public RelayCommand PushCommand
        {
            get { return _pushCommand ??= new RelayCommand(()=>Push()); }
        }

        private void Push()
        {
            IsPushButtonEnable = false;
            OutputInfo = string.Empty;
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Push();
                IsPushButtonEnable = true;
            });
        }

        #endregion

        #region OpenTerminal Command

        private RelayCommand _openTerminalCommand;

        public RelayCommand OpenTerminalCommand
        {
            get { return _openTerminalCommand ??= new RelayCommand(async () => await OpenTerminal()); }
        }

        private RelayCommand _fetchCommand;

        public RelayCommand FetchCommand
        {
            get { return _fetchCommand ??= new RelayCommand(Fetch); }
        }

        private void Fetch()
        {
            IsFetchButtonEnable = false;
            OutputInfo = string.Empty;
            Task.Run(() =>
            {
                var currentBranch = _gitProcessor.Fetch();
                IsFetchButtonEnable = true;
            });
        }

        private RelayCommand _createBranchCommand;

        public RelayCommand CreateBranchCommand
        {
            get { return _createBranchCommand ??= new RelayCommand(CreateBranch); }
        }

        private async void CreateBranch()
        {
            IsCreateButtonEnable = false;
            var currentBranch = await _gitProcessor.GetBranch();
            OutputInfo = string.Empty;
            await _gitProcessor.CreateBranch(currentBranch, _newBranchName);
            IsCreateButtonEnable = true;
        }

        private async Task OpenTerminal()
        {
            var selectedRepository = Repositories[int.Parse(SelectedRepoIndex)];
            await _gitProcessor.OpenTerminal(selectedRepository);
        }

        #endregion

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
                    Repositories = new ObservableCollection<string>();

                    foreach (var repository in repositories)
                    {
                        Repositories.Add(repository);

                        if (repository.Equals(defaultRepository, StringComparison.OrdinalIgnoreCase))
                        {
                            SelectedRepoIndex = (Repositories.Count - 1).ToString();
                        }
                    }
                }
            });

            Task.Run(() =>
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
                                var currentBranch = remoteBranch.Replace("*", string.Empty);
                                Branches.Add(currentBranch.Trim());
                                SelectedBranchIndex = (Branches.Count - 1).ToString();
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

        private void OnOutput(string line)
        {
            OutputInfo += line + Environment.NewLine;
        }
    }
}
