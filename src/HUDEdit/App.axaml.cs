using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Sentry;
using System.IO;
using System.Reflection;

namespace HUDEdit;

public partial class App : Application
{
    public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public static IConfiguration Config { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public App()
    {
        // Load configuration from appconfig.json
        Config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Dispatcher.UIThread.UnhandledException += App_DispatcherUnhandledException;
        SentrySdk.Init(o =>
        {
            // Tells which project in Sentry to send events to:
            o.Dsn = Config["Sentry:Dsn"];
            // When configuring for the first time, to see what the SDK is doing:
            o.Debug = true;
        });

        // Set the logger
        XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), new FileInfo("log4net.config"));
        Logger.Info("=======================================================");
        Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SentrySdk.CaptureException(e.Exception);

        // If you want to avoid the application from crashing:
        e.Handled = true;
    }
}
