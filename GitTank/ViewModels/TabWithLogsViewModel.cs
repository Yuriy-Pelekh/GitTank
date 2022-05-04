namespace GitTank.ViewModels
{
    public class TabWithLogsViewModel : BaseViewModel
    {
        public string Header { get; set; }

        private string _outputInfo;
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
    }
}