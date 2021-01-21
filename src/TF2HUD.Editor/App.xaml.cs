using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;

namespace TF2HUD.Editor
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            base.OnStartup(e);
        }
    }
}