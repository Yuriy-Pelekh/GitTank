namespace GitTank.ViewModels
{
    public class RepositoryTileViewModel : BaseViewModel
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
