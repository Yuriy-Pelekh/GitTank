using GitTank.Loggers;
using GitTank.ViewModels;
using System.Windows;
using GitTank.Configuration;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for CreateBranchWindow.xaml
    /// </summary>
    public partial class CreateBranchWindow
    {
        public CreateBranchWindow(ISettings settings, ILogger logger)
        {
            InitializeComponent();
            DataContext = new CreateBranchViewModel(settings, logger);
            ((CreateBranchViewModel)DataContext).CreateBranch += () =>
            {
                Application.Current.Dispatcher.Invoke(Close);
            };

            Loaded += CreateBranchWindow_Loaded;
        }

        private void CreateBranchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BranchNameTextBox.Focus();
        }
    }
}
