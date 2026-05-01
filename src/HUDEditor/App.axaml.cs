using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.ViewModels;
using HUDEditor.Views;
using log4net;
using log4net.Config;
using Sentry;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace HUDEditor;

public partial class App : Application
{
    public static readonly ILog Logger = LogManager.GetLogger(typeof(App));
    public static ConfigurationModel Config { get; private set; } = null!;
    public static string HudPath { get; set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        Dispatcher.UIThread.UnhandledException += App_DispatcherUnhandledException;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowVm = new MainWindowViewModel();
            var splashScreenVm = new SplashScreenViewModel();
            var splashScreen = new SplashScreenView
            {
                DataContext = splashScreenVm
            };
            desktop.MainWindow = splashScreen;
            splashScreen.Show();

            try
            {
                Setup();
                await Task.Delay(1000, splashScreenVm.CancellationToken);
                await mainWindowVm.LoadHUDs();
                await Task.Delay(1000, splashScreenVm.CancellationToken);
                await splashScreenVm.DownloadImages(mainWindowVm.HUDList);
            }
            catch (TaskCanceledException)
            {
                splashScreen.Close();
                return;
            }

            var mainWindow = new MainWindow
            {
                DataContext = mainWindowVm
            };
            desktop.MainWindow = mainWindow;
            mainWindowVm.TopLevel = mainWindow;
            mainWindowVm.PropertyChanged += mainWindow.MainWindowViewModelPropertyChanged;
            mainWindow.Show();
            splashScreen.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Setup()
    {
        // Setup Logger
        XmlConfigurator.Configure(new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));
        Logger.Info("=======================================================");
        Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
        Logger.Info("------");

        // Load Configuration
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        try
        {
            if (!File.Exists(configPath))
                Logger.Warn($"Config file not found at \"{configPath}\".");
            else
            {
                var json = File.ReadAllText(configPath);
                Config = JsonSerializer.Deserialize<ConfigurationModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ConfigurationModel();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load \"{configPath}\": {ex.Message}. Using defaults.");
        }
        Config ??= new ConfigurationModel();

        // Setup Sentry — SENTRY_DSN is applied by a GitHub Action during packaging.
        if (!Config.ConfigSettings.UserPrefs.DisableSentry)
        {
            var dsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? Config.ConfigSettings.AppConfig.SentryDsn;
            if (!string.IsNullOrWhiteSpace(dsn))
            {
                SentrySdk.Init(o =>
                {
                    o.Dsn = dsn;
                    o.Debug = false;
                });
            }
            else
                Logger.Warn("Sentry DSN not configured — error reporting disabled.");
        }

        // Set user preferences
        var language = Config.ConfigSettings.UserPrefs.Language;
        var systemLanguage = Utilities.GetSystemLanguage();
        if (string.IsNullOrEmpty(language) || (language == "en-US" && systemLanguage != "en-US"))
        {
            language = systemLanguage;
            Config.ConfigSettings.UserPrefs.Language = language;
            SaveConfiguration();
        }
        Assets.Resources.Culture = new CultureInfo(language);
        HudPath = Config.ConfigSettings.UserPrefs.HUDDirectory;
    }

    private void App_DispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SentrySdk.CaptureException(e.Exception);

        // Prevent the application from crashing
        e.Handled = true;
    }

    public static void SaveConfiguration()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var tempPath = configPath + ".tmp";

        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Write to a temp file first, then replace the real config to prevent a crash mid-write.
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, configPath, overwrite: true);
    }
}