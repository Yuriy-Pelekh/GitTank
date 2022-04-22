using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GitTank.Loggers;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public IServiceProvider ServiceProvider { get; private set; }

        private ILogger _logger;
        private ILogger Logger => _logger ??= new GeneralLogger();

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

            Logger.Debug("Starting application");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            Logger.Debug("Configuring services");
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            Logger.Debug("Creating main window");
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Logger.Debug("Application started");
        }

        private IConfiguration AddConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

#if DEBUG
            Logger.Debug("Use development configuring");
            builder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
#else
            Logger.Debug("Use production configuring");
            builder.AddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true);
#endif

            return builder.Build();
        }

        private ILogger AddLogger()
        {
            return Logger;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(AddConfiguration());
            services.AddSingleton(AddLogger());
            services.AddTransient(typeof(MainWindow));
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error("OnCurrentDomainUnhandledException", e.ExceptionObject as Exception);
            Debugger.Break();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error("OnDispatcherUnhandledException", e.Exception);
            e.Handled = true;
        }

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error("OnApplicationDispatcherUnhandledException", e.Exception);
            Debugger.Break();
        }

        private void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error("OnTaskSchedulerUnobservedTaskException", e.Exception);
            Debugger.Break();
        }
    }
}
