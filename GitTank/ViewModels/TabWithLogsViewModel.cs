namespace GitTank.ViewModels
{
    public class TabWithLogsViewModel : BaseViewModel
    {
        private string _header;
        private string _outputInfo;

        public string Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    _header = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OutputInfo
        {
            get => _outputInfo;
            set
            {
                if (_outputInfo != value)
                {
                    _outputInfo = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
