using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Sentry;
using HUDEdit.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using HUDEdit.ViewModels;
using System.Globalization;
using HUDEdit.Views;

namespace HUDEdit;

public partial class App : Application
{
    public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public static ConfigurationModel Config { get; private set; }
    public MainWindowViewModel MainWindowViewModel { get; } = new MainWindowViewModel();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = MainWindowViewModel
            };
            MainWindowViewModel.PropertyChanged += mainWindow.MainWindowViewModelPropertyChanged;
            desktop.MainWindow = mainWindow;
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

        // Set user language
        Assets.Resources.Culture = new CultureInfo(Config.ConfigSettings.UserPrefs.Language);
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
