using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.Properties;
using HUDEditor.ViewModels;
using log4net;
using log4net.Config;
using Octokit;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string HudSelection = Settings.Default.hud_selected;
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public MainWindow()
        {
            // Initialize the logger
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), new FileInfo("log4net.config"));
            Logger.Info("=======================================================");
            Logger.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");

            // Initialize the main window
            InitializeComponent();
            var mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
            DataContext = mainWindowViewModel;

            // Check for tf/custom directory
            SetupDirectory();

#if !DEBUG
            // Check for updates
            UpdateAppSchema(true);
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
        public static void SetupDirectory(bool userSet = false)
        {
            if ((Utilities.SearchRegistry() || Utilities.CheckUserPath(HudPath)) && !userSet) return;

            // Display a folder browser, ask the user to provide the tf/custom directory.
            Logger.Info("Target directory not set. Asking user to provide it.");
            using (var browser = new FolderBrowserDialog
            {
                Description = Properties.Resources.info_path_browser,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            })
            {
                // Loop until the user provides a valid tf/custom directory, unless they cancel out.
                while (!browser.SelectedPath.EndsWith("tf\\custom"))
                {
                    if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (browser.SelectedPath.EndsWith("tf\\custom"))
                        {
                            Settings.Default.hud_directory = browser.SelectedPath;
                            Settings.Default.Save();
                            HudPath = Settings.Default.hud_directory;
                            Logger.Info("Target directory set to: " + Settings.Default.hud_directory);
                        }
                        else
                        {
                            ShowMessageBox(MessageBoxImage.Error, Properties.Resources.info_path_invalid);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Check one more time if a valid directory has been set.
            if (Utilities.CheckUserPath(HudPath)) return;
            Logger.Info("Target directory still not set. Closing.");
            ShowMessageBox(MessageBoxImage.Warning, Utilities.GetLocalizedString("error_app_directory"));
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Displays a set type of message box to the user.
        /// </summary>
        public static MessageBoxResult ShowMessageBox(MessageBoxImage type, string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            switch (type)
            {
                case MessageBoxImage.Error:
                    Logger.Error(message);
                    break;

                case MessageBoxImage.Warning:
                    Logger.Warn(message);
                    break;
            }

            return MessageBox.Show(message, string.Empty, buttons, type);
        }

        /// <summary>
        /// Checks if the selected HUD is installed correctly.
        /// </summary>
        /// <returns>True if the selected hud is installed.</returns>
        public static bool CheckHudInstallation(HUD hud)
        {
            return hud != null &&
                HudPath != null &&
                Directory.Exists(HudPath) &&
                Utilities.CheckUserPath(HudPath) &&
                Directory.Exists($"{HudPath}\\{hud.Name}");
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
                var remoteFiles = (await Utilities.Fetch<GitJson[]>(Settings.Default.json_list)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();

                foreach (var remoteFile in remoteFiles)
                {
                    var localFilePath = $"JSON\\{remoteFile.Name}";
                    bool newFile = false, fileChanged = false;

                    if (!File.Exists(localFilePath))
                        newFile = true;
                    else
                        fileChanged = remoteFile.SHA != Utilities.GitHash(localFilePath);

                    if (!newFile && !fileChanged) continue;
                    Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                    downloads.Add(Utilities.DownloadFile(remoteFile.Download, localFilePath));
                }

                // Remove HUD JSONs that aren't available online.
                foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
                {
                    if (remoteFiles.Count((x) => x.Name == localFile.Name) == 0)
                    {
                        Logger.Info($"Deleting {localFile.Name}");
                        File.Delete(localFile.FullName);
                    }
                }

                await Task.WhenAll(downloads);
                if (Convert.ToBoolean(downloads.Count))
                {
                    if (!silent) if (ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                    Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                    Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                    Environment.Exit(0);
                }
                else
                {
                    if (!silent) ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update_none);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
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
                Logger.Info($"Checking for app update. Latest version is {latestVersion.TagName}");

                if (appVersion.Equals(latestVersion.TagName)) return;
                Logger.Info($"Update available from {appVersion} -> {latestVersion.TagName}");
                if (ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_app_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                Utilities.OpenWebpage(Settings.Default.app_update);

            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Console.WriteLine(e);
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}