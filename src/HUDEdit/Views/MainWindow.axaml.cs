using HUDEdit.Classes;
using HUDEdit.ViewModels;
using Octokit;
using HUDEdit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;

namespace HUDEdit.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public static string HudPath = App.Config.ConfigSettings.UserPrefs.HUDDirectory;

    public MainWindow()
    {
        InitializeComponent();

        var mainWindowViewModel = new MainWindowViewModel();
        mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
        Utilities.SetupDirectoryAsync(this);

        // Check for updates
        if (App.Config.ConfigSettings.UserPrefs.AutoUpdate == true) UpdateAppSchema(true);
    }

    private void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedHud))
        {
            App.Config.ConfigSettings.UserPrefs.SelectedHUD = ((MainWindowViewModel)sender).SelectedHud?.Name ?? string.Empty;
        }
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
                var localFilePath = $"JSON/{remoteFile.Name}";
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
                if (!silent) if (await Utilities.ShowPromptBox(Assets.Resources.info_hud_update) == ButtonResult.No) return;
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            }
            else
            {
                if (!silent) await Utilities.ShowMessageBox(Assets.Resources.info_hud_update_none);
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
            var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var latestVersion = await new GitHubClient(new ProductHeaderValue("TF2HUD.Editor")).Repository.Release.GetLatest("CriticalFlaw", "TF2HUD.Editor");
            App.Logger.Info($"Checking for app update. Latest version is {latestVersion.TagName}");

            // Parse the remote version (remove a leading 'v' if present)
            if (!Version.TryParse(latestVersion.TagName.TrimStart('v'), out var remoteVersion))
            {
                App.Logger.Warn($"Failed to parse remote version: {latestVersion.TagName}");
                return;
            }

            // Only update if remote version is *greater than* the local version
            if (remoteVersion > localVersion)
            {
                App.Logger.Info($"Update available from {localVersion} -> {remoteVersion}");
                if (await Utilities.ShowPromptBox(Assets.Resources.info_app_update) == ButtonResult.No) return;
                Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.LatestUpdateURL);
            }
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
        }
    }
}