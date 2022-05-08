using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Windows;

namespace GitTank
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(configuration, logger);
            ((SettingsViewModel)DataContext).OnClick += () =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { Close(); }));
            };
        }
    }
}
