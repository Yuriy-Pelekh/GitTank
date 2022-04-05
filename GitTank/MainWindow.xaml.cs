using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IConfiguration configuration)
        {
            InitializeComponent();

            DataContext = new MainViewModel(configuration);
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
