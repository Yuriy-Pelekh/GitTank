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
        private readonly ILogger _logger;

        public MainWindow(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            Closed += OnMainWindowClosed;

            InitializeComponent();

            DataContext = new MainViewModel(configuration, logger);
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            LogContext.PushProperty(Constants.SourceContext, GetType().Name);
            _logger.Information("Application was closed");
        }

        private void OnTextBoxOutputTextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }
    }
}
