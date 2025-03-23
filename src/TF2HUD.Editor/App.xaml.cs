using HUDEditor.Properties;
using log4net;
using log4net.Config;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private App()
        {
            // Set the logger
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), new FileInfo("log4net.config"));
            Logger.Info("=======================================================");
            Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");

            // Set the language
            LocalizeDictionary.Instance.Culture = new CultureInfo(Settings.Default.user_language);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            base.OnStartup(e);
        }
    }
}