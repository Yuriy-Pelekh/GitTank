using System.Windows;
using System.Windows.Controls;
using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();

            DataContext = new MainViewModel(configuration, logger);
        }

        private void OnTextBoxOutputTextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        /* private void OnButtonBranchesClick(object sender, RoutedEventArgs e)
         {
             ((UIElement) sender).IsEnabled = false;
             TextBoxOutput.Text = string.Empty;
             Task.Run(() =>
             {
                 var remoteBranches = _gitProcessor.Branches();
                 Dispatcher.BeginInvoke(new Action(() => ((UIElement) sender).IsEnabled = true), DispatcherPriority.Background);
             });
         }*/
    }
}
