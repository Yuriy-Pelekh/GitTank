using System.Threading.Tasks;
using GitTank.Common;
using GitTank.Configuration;
using GitTank.Core;
using GitTank.Loggers;
using Microsoft.Toolkit.Mvvm.Input;

namespace GitTank.ViewModels
{
    public class CreateBranchViewModel : BaseViewModel
    {
        public event CreateBranchEventHandler CreateBranch;

        private RelayCommand _createBranchCommand;

        private readonly GitProcessor _gitProcessor;
        private bool _isCreateButtonEnabled = true;
        private string _newBranchName;

        public CreateBranchViewModel(ISettings settings, ILogger logger)
        {
            _gitProcessor = new GitProcessor(settings, logger);
        }

        public bool IsCreateButtonEnabled
        {
            get => _isCreateButtonEnabled;
            set
            {
                if (_isCreateButtonEnabled != value)
                {
                    _isCreateButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public RelayCommand CreateBranchCommand
        {
            get
            {
                async void Execute() => await OnCreateBranch();
                return _createBranchCommand ??= new RelayCommand(Execute);
            }
        }

        private async Task OnCreateBranch()
        {
            IsCreateButtonEnabled = false;
            await _gitProcessor.CreateBranch(NewBranchName);
            IsCreateButtonEnabled = true;
            CreateBranch?.Invoke();
        }
    }
}
