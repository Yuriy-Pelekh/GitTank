using System.Threading.Tasks;
using GitTank.Loggers;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;

namespace GitTank.ViewModels
{
    public class CreateBranchViewModel : BaseViewModel
    {
        public delegate void OnClickEventHandler();
        public event OnClickEventHandler OnClick;

        private readonly GitProcessor _gitProcessor;
        private bool _isCreateButtonEnabled = true;
        private string _newBranchName;

        public CreateBranchViewModel(IConfiguration configuration, ILogger logger)
        {
            _gitProcessor = new GitProcessor(configuration, logger);
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

        private RelayCommand _createBranchCommand;

        public RelayCommand CreateBranchCommand
        {
            get { return _createBranchCommand ??= new RelayCommand(CreateBranch); }
        }

        private void CreateBranch()
        {
            Task.Run(() =>
            {
                IsCreateButtonEnabled = false;
                var branch = _gitProcessor.CreateBranch(NewBranchName);
                IsCreateButtonEnabled = true;
                OnClick?.Invoke();
            });
        }

        private RelayCommand _checkTextBoxCommand;

        public RelayCommand CheckTextBoxCommand
        {
            get { return _checkTextBoxCommand ??= new RelayCommand(CheckTextBox); }
        }

        private void CheckTextBox()
        {
            if (!string.IsNullOrEmpty(NewBranchName))
            {
                IsCreateButtonEnabled = true;
            }
        }
    }
}
