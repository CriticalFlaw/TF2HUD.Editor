using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using HUDEditor.Classes;
using HUDEditor.Properties;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using WPFLocalizeExtension.Engine;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;

        private App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            LocalizeDictionary.Instance.Culture = new CultureInfo(Settings.Default.user_language);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            ConfigureLogging(services);
            services.AddSingleton<Utilities>();
            services.AddSingleton<Notifier>();
            services.AddSingleton<HudDirectory>();
            services.AddSingleton<MainWindow>();
        }

        private void ConfigureLogging(ServiceCollection services)
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            services.AddScoped(factory => LogManager.GetLogger(GetType()));
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}