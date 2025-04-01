using HUDEdit.Classes;
using HUDEdit.ViewModels;
using Octokit;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace HUDEdit;

public partial class MainWindow : Avalonia.Controls.Window
{
    // TODO: Store these in the config instead of here.
    public static string HudSelection = App.Config.ConfigSettings.UserPrefs.SelectedHUD;
    public static string HudPath = App.Config.ConfigSettings.UserPrefs.HUDDirectory;

    public MainWindow()
    {
        InitializeComponent();

        var mainWindowViewModel = new MainWindowViewModel();
        mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;

        // Check for tf/custom directory
        SetupDirectoryAsync();

#if !DEBUG
        // Check for updates
        if (App.Config.ConfigSettings.UserPrefs.AutoUpdate == true) UpdateAppSchema(true);
#endif
    }

    private void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedHud))
        {
            HudSelection = ((MainWindowViewModel)sender).SelectedHud?.Name ?? string.Empty;
        }
    }

    /// <summary>
    /// Setups the target directory (tf/custom).
    /// </summary>
    /// <param name="userSet">If true, prompts the user to select the tf/custom using the folder browser.</param>
    public static async Task SetupDirectoryAsync(bool userSet = false)
    {
        if ((Utilities.SearchRegistry() || Utilities.CheckUserPath(HudPath)) && !userSet) return;

        // TODO: Loop until the user provides a valid tf/custom directory, unless they cancel out.
        // Display a folder browser, ask the user to provide the tf/custom directory.
        //App.Logger.Info("Target directory not set. Asking user to provide it.");
        //var browser = new OpenFolderDialog
        //{
        //    Title = Assets.Resources.info_path_browser,
        //};
        //if (userPath.Value.EndsWith("tf\\custom"))
        //{
        //    HudPath = userPath.Value;
        //    App.Config.ConfigSettings.UserPrefs.HUDDirectory = userPath;
        //    App.SaveConfiguration();
        //    HudPath = App.Config.ConfigSettings.UserPrefs.HUDDirectory;
        //    App.Logger.Info("Target directory set to: " + App.Config.ConfigSettings.UserPrefs.HUDDirectory);
        //}
        //else
        //{
        //    Utilities.ShowMessageBox(MessageBoxImage.Error, Assets.Resources.info_path_invalid);
        //}

        // Check one more time if a valid directory has been set.
        if (Utilities.CheckUserPath(HudPath)) return;
        App.Logger.Info("Target directory still not set. Closing.");
        Utilities.ShowMessageBox(MessageBoxImage.Warning, Utilities.GetLocalizedString("error_app_directory"));
        Environment.Exit(0);
    }

    /// <summary>
    /// Synchronizes the local HUD schema files with the latest versions on GitHub.
    /// </summary>
    /// <param name="silent">If true, the user will not be notified if there are no updates on startup.</param>
    public static async void UpdateAppSchema(bool silent = true)
    {
        try
        {
            // Create the schema folder if it does not exist.
            if (!Directory.Exists("JSON")) Directory.CreateDirectory("JSON");

            var downloads = new List<Task>();
            var remoteFiles = (await Utilities.Fetch<GitJson[]>(App.Config.ConfigSettings.AppConfig.JsonListURL)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();

            foreach (var remoteFile in remoteFiles)
            {
                var localFilePath = $"JSON\\{remoteFile.Name}";
                bool newFile = false, fileChanged = false;

                if (!File.Exists(localFilePath))
                    newFile = true;
                else
                    fileChanged = remoteFile.SHA != Utilities.GitHash(localFilePath);

                if (!newFile && !fileChanged) continue;
                App.Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                downloads.Add(Utilities.DownloadFile(remoteFile.Download, localFilePath));
            }

            // Remove HUD JSONs that aren't available online.
            foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
            {
                if (remoteFiles.Count((x) => x.Name == localFile.Name) == 0)
                {
                    App.Logger.Info($"Deleting {localFile.Name}");
                    File.Delete(localFile.FullName);
                }
            }

            await Task.WhenAll(downloads);
            if (Convert.ToBoolean(downloads.Count))
            {
                if (!silent) if (Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_hud_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            }
            else
            {
                if (!silent) Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_hud_update_none);
            }
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
        }
        finally
        {
            UpdateAppVersion();
        }
    }

    /// <summary>
    /// Checks if there's a new version of the app available.
    /// </summary>
    public static async void UpdateAppVersion()
    {
        try
        {
            string appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 3);
            var latestVersion = await new GitHubClient(new ProductHeaderValue("TF2HUD.Editor")).Repository.Release.GetLatest("CriticalFlaw", "TF2HUD.Editor");
            App.Logger.Info($"Checking for app update. Latest version is {latestVersion.TagName}");

            if (appVersion.Equals(latestVersion.TagName)) return;
            App.Logger.Info($"Update available from {appVersion} -> {latestVersion.TagName}");
            if (Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_app_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.LatestUpdateURL);

        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
        }
    }
}