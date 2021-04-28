using System;
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
        private readonly GitProcessor _gitProcessor;

        public MainWindow(IConfiguration configuration)
        {
            InitializeComponent();
            Loaded += OnLoaded;

            DataContext = new MainViewModel();
            _gitProcessor = new GitProcessor(configuration);
            _gitProcessor.Output += OnOutput;
        }

        ~MainWindow()
        {
            _gitProcessor.Output -= OnOutput;
            Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Branches().Result;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var remoteBranch in remoteBranches.Split(Environment.NewLine))
                    {
                        if (remoteBranch.StartsWith("*"))
                        {
                            var currentBranch = remoteBranch.Replace("*", string.Empty);
                            ComboBoxBranches.Items.Add(currentBranch.Trim());
                            ComboBoxBranches.SelectedIndex = ComboBoxBranches.Items.Count - 1;
                        }
                        else
                        {
                            ComboBoxBranches.Items.Add(remoteBranch.Trim());
                        }
                    }
                }), DispatcherPriority.Background);
            });
        }

        private void OnOutput(string line)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBoxOutput.Text += line + Environment.NewLine;
                TextBoxOutput.ScrollToEnd();
            }), DispatcherPriority.Background);
        }

        private void OnButtonBranchClick(object sender, RoutedEventArgs e)
        {
            ((UIElement) sender).IsEnabled = false;
            TextBoxOutput.Text = string.Empty;
            Task.Run(() =>
            {
                var currentBranch = _gitProcessor.GetBranch();
                Dispatcher.BeginInvoke(new Action(() => ((UIElement) sender).IsEnabled = true), DispatcherPriority.Background);
            });
        }

        private void OnButtonUpdateClick(object sender, RoutedEventArgs e)
        {
            ((UIElement) sender).IsEnabled = false;
            TextBoxOutput.Text = string.Empty;
            Task.Run(() =>
            {
                var output = _gitProcessor.Update();
                Dispatcher.BeginInvoke(new Action(() => ((UIElement) sender).IsEnabled = true), DispatcherPriority.Background);
            });
        }

        private void OnButtonBranchesClick(object sender, RoutedEventArgs e)
        {
            ((UIElement) sender).IsEnabled = false;
            TextBoxOutput.Text = string.Empty;
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Branches();
                Dispatcher.BeginInvoke(new Action(() => ((UIElement) sender).IsEnabled = true), DispatcherPriority.Background);
            });
        }

        private void OnButtonCheckoutClick(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).IsEnabled = false;
            TextBoxOutput.Text = string.Empty;
            var selectedItem = (string)ComboBoxBranches.SelectedItem;
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Checkout(selectedItem);
                Dispatcher.BeginInvoke(new Action(() => ((UIElement)sender).IsEnabled = true), DispatcherPriority.Background);
            });
        }

        private void OnButtonSyncClick(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).IsEnabled = false;
            TextBoxOutput.Text = string.Empty;
            Task.Run(() =>
            {
                var remoteBranches = _gitProcessor.Sync();
                Dispatcher.BeginInvoke(new Action(() => ((UIElement)sender).IsEnabled = true), DispatcherPriority.Background);
            });
        }
    }
}
