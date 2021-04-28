using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

namespace GitTank
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

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
    }
}
