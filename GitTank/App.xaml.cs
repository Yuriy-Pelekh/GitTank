using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            // Catch exceptions from the main UI dispatcher thread in the WPF application.
            Current.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
            // Catch exceptions from within each AppDomain that uses a task scheduler for asynchronous operations.
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private IConfiguration AddConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

#if DEBUG
            builder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
#else
            builder.AddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true);
#endif

            return builder.Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(AddConfiguration());
            services.AddTransient(typeof(MainWindow));
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Debugger.Break();
        }
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
            e.Handled = true;
        }

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
            Debugger.Break();
        }

        private void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
            Debugger.Break();
        }
    }
}
