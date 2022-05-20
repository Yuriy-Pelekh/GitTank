using GitTank.Loggers;
using GitTank.ViewModels;
using System.Windows;
using GitTank.Configuration;

namespace GitTank
{
    public partial class SettingsWindow
    {
        public SettingsWindow(ISettings settings, ILogger logger)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(settings, logger);
            ((SettingsViewModel)DataContext).Click += () =>
            {
                Application.Current.Dispatcher.Invoke(Close);
            };
        }
    }
}
