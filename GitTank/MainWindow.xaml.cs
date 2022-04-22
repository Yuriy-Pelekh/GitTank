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
    }
}
