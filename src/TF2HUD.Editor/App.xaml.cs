using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using HUDEditor.Properties;
using log4net;
using log4net.Config;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        App()
        {
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new CultureInfo(Settings.Default.user_language);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            base.OnStartup(e);
        }
    }
}