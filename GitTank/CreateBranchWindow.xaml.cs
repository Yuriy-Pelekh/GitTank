using System;
using GitTank.Loggers;
using GitTank.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for CreateBranchWindow.xaml
    /// </summary>
    public partial class CreateBranchWindow : Window
    {
        public CreateBranchWindow(IConfiguration configuration, ILogger logger)
        {
            InitializeComponent();
            DataContext = new CreateBranchViewModel(configuration, logger);
            ((CreateBranchViewModel)DataContext).OnClick += () =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { Close(); }));
            };

            Loaded += CreateBranchWindow_Loaded;
        }

        private void CreateBranchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BranchNameTextBox.Focus();    
        }
    }
}
