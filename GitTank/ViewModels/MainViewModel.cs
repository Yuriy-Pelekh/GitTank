using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GitTank.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly GitProcessor _gitProcessor;
        private string selectedRepoIndex;
        private string selectedBranchIndex;
        private string outputInfo;
        private bool isUpdateButtonEnable = true;
        private bool isBranchButtonEnable = true;
        private bool isCheckoutButtonEnable = true;
        private bool isSyncButtonEnable = true;
        private bool isPushButtonEnable = true;

        public ObservableCollection<string> Repositories { get; set; }
        public ObservableCollection<string> Branches { get; set; }

        public string SelectedRepoIndex
        {
            get { return selectedRepoIndex; }
            set
            {
                selectedRepoIndex = value;
                OnPropertyChanged("SelectedRepoIndex");
            }
        }

        public string SelectedBranchIndex
        {
            get { return selectedBranchIndex; }
            set
            {
                selectedBranchIndex = value;
                OnPropertyChanged("SelectedBranchIndex");
            }
        }

        public string OutputInfo
        {
            get { return outputInfo; }
            set
            {
                outputInfo = value;
                OnPropertyChanged("OutputInfo");
            }
        }

        public bool IsUpdateButtonEnable
        {
            get { return isUpdateButtonEnable; }
            set
            {
                isUpdateButtonEnable = value;
                OnPropertyChanged("IsUpdateButtonEnable");
            }
        }

        public bool IsBranchButtonEnable
        {
            get { return isBranchButtonEnable; }
            set
            {
                isBranchButtonEnable = value;
                OnPropertyChanged("IsBranchButtonEnable");
            }
        }

        public bool IsCheckoutButtonEnable
        {
            get { return isCheckoutButtonEnable; }
            set
            {
                isCheckoutButtonEnable = value;
                OnPropertyChanged("IsCheckoutButtonEnable");
            }
        }

        public bool IsSyncButtonEnable
        {
            get { return isSyncButtonEnable; }
            set
            {
                isSyncButtonEnable = value;
                OnPropertyChanged("IsSyncButtonEnable");
            }
        }

        public bool IsPushButtonEnable
        {
            get { return isPushButtonEnable; }
            set
            {
                isPushButtonEnable = value;
                OnPropertyChanged("IsPushButtonEnable");
            }
        }

        #region Branch Command 
        private RelayCommand branchCommand;

        public RelayCommand BranchCommand
        {
            get
            {
                return branchCommand ?? (branchCommand = new RelayCommand(() => Branch()));
            }
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
        private RelayCommand updateCommand;

        public RelayCommand UpdateCommand
        {
            get
            {
                return updateCommand ?? (updateCommand = new RelayCommand(() => Update()));
            }

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
        private RelayCommand checkoutCommand;

        public RelayCommand CheckoutCommand
        {
            get
            {
                return checkoutCommand ?? (checkoutCommand = new RelayCommand(() => Checkout()));
            }

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
        private RelayCommand syncCommand;

        public RelayCommand SyncCommand
        {
            get
            {
                return syncCommand ?? (syncCommand = new RelayCommand(() => Sync()));
            }

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
        private RelayCommand pushCommand;

        public RelayCommand PushCommand
        {
            get
            {
                return pushCommand ?? (pushCommand = new RelayCommand(() => Push()));
            }

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
        public MainViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _gitProcessor = new GitProcessor(configuration);
            _gitProcessor.Output += OnOutput;
            OnLoaded();
        }
        private void OnLoaded()
        {
            Task.Run(() =>
            {
                var defaultRepository = _configuration.GetValue<string>("appSettings:defaultRepository");
                var repositories = _configuration.GetSection("appSettings:repositories")
                    .GetChildren()
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
            });
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Branches().Result;
                Branches = new ObservableCollection<string>(); 
                foreach (var remoteBranch in remoteBranches.Split(Environment.NewLine))
                {
                    if (!string.IsNullOrWhiteSpace(remoteBranch?.Trim()))
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
            });
        }

        private void OnOutput(string line)
        {
            OutputInfo += line + Environment.NewLine;
        }

    }
}
