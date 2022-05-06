using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for CreateBranchWindow.xaml
    /// </summary>
    public partial class CreateBranchWindow
    {
        public CreateBranchWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();
            DataContext = new CreateBranchViewModel(configuration, logger);
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
