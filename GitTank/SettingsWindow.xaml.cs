using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GitTank
{
    public partial class SettingsWindow
    {
        public SettingsWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(configuration, logger);
            ((SettingsViewModel)DataContext).Click += () =>
            {
                Application.Current.Dispatcher.Invoke(Close);
            };
        }
    }
}
