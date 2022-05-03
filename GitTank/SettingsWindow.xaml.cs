using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GitTank
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();
            var settingsViewModel = new SettingsViewModel(configuration, logger);
            this.DataContext = settingsViewModel;
            settingsViewModel.ClosingRequest += (sender, e) => this.Close();
        }
    }
}
