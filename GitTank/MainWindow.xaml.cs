using System;
using System.Windows;
using System.Windows.Controls;
using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using Serilog.Context;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger _generalLogger;
        public MainWindow(IConfiguration configuration, ILogger logger)
        {
            Closed += MainWindowClosed;
            InitializeComponent();

            DataContext = new MainViewModel(configuration, logger);
            _generalLogger = logger;
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            LogContext.PushProperty(Constants.SourceContext, GetType().Name);
            _generalLogger.Information("Application was closed");
        }

        private void OnTextBoxOutputTextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}
