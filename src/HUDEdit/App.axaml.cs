using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using log4net;
using log4net.Config;
using HUDEdit.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using HUDEdit.ViewModels;
using System.Globalization;
using HUDEdit.Views;
using System.Threading.Tasks;

namespace HUDEdit;

public partial class App : Application
{
    public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public static ConfigurationModel Config { get; private set; }
    public static string HudPath { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
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
                splashScreenVm.StartupMessage = Assets.Resources.ui_splash_initialize;
                await Task.Delay(1000, splashScreenVm.CancellationToken);

                await mainWindowVm.LoadHUDs();
                splashScreenVm.StartupMessage = Assets.Resources.ui_splash_schema;
                await Task.Delay(1000, splashScreenVm.CancellationToken);

                splashScreenVm.StartupMessage = Assets.Resources.ui_splash_images;
                await mainWindowVm.DownloadImages();
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

        // Load Configuration
        var json = File.ReadAllText("appsettings.json");
        Config = JsonSerializer.Deserialize<ConfigurationModel>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ConfigurationModel();

        // Set user preferences
        Assets.Resources.Culture = new CultureInfo(Config.ConfigSettings.UserPrefs.Language);
        HudPath = Config.ConfigSettings.UserPrefs.HUDDirectory;
    }

    public static void SaveConfiguration()
    {
        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText("appsettings.json", json);
    }
}