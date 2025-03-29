using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Sentry;
using Shared.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace HUDEdit;

public partial class App : Application
{
    public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public static ConfigurationModel Config { get; private set; }
    public static IConfiguration Secrets { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //Shared.Resources.Resources.Culture = new CultureInfo("en");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            //DataContext = new MainWindowViewModel();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public App()
    {
        // Setup Logger
        XmlConfigurator.Configure(new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));
        Logger.Info("=======================================================");
        Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");

        // Load Configuration
        var json = File.ReadAllText("appsettings.json");
        Config = JsonSerializer.Deserialize<ConfigurationModel>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ConfigurationModel();

        // Setup Sentry
        if (File.Exists("secrets.json"))
        {
            Secrets = new ConfigurationBuilder().AddJsonFile("secrets.json").Build();

            Dispatcher.UIThread.UnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init(o =>
            {
                o.Dsn = Secrets["Sentry:Dsn"];
                o.Debug = true;
            });
        }
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SentrySdk.CaptureException(e.Exception);

        // Prevent the application from crashing
        e.Handled = true;
    }

    public static void SaveConfiguration()
    {
        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
        {
            WriteIndented = true // Pretty-print the JSON
        });

        File.WriteAllText("appsettings.json", json);
    }
}
