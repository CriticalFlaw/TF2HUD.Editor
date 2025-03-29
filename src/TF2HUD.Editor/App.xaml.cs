using HUDEditor.Properties;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Sentry;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using WPFLocalizeExtension.Engine;

namespace HUDEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public static IConfiguration Config { get; private set; }

    private App()
    {
        if (File.Exists("secrets.json"))
        {
            // Load configuration from appconfig.json
            Config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = Config["Sentry:Dsn"];
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = true;
            });
        }

        // Set the logger
        XmlConfigurator.Configure(new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));
        Logger.Info("=======================================================");
        Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");

        // Set the language
        LocalizeDictionary.Instance.Culture = new CultureInfo(Settings.Default.user_language);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SentrySdk.CaptureException(e.Exception);

        // If you want to avoid the application from crashing:
        e.Handled = true;
    }
}