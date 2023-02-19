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
using AutoUpdaterDotNET;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.Properties;
using HUDEditor.ViewModels;
using log4net;
using log4net.Config;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace HUDEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string HudSelection = Settings.Default.hud_selected;
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public MainWindow()
        {
            // Initialize the logger.
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("=======================================================");
            Logger.Info("Initializing.");

            // Get the list of HUD JSONs.

            InitializeComponent();

            var mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
            DataContext = mainWindowViewModel;

            // Setup the user interface.
            SetupDirectory();

            // Check for app updates.
            if (Settings.Default.app_update_auto == true)
                CheckSchemaUpdates();

            Logger.Info("Checking for app updates.");
            AutoUpdater.Start(Settings.Default.app_update);
        }

        private void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedHud))
            {
                HudSelection = ((MainWindowViewModel)sender).SelectedHud?.Name ?? string.Empty;
            }
        }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        public static void SetupDirectory(bool userSet = false)
        {
            if ((Utilities.SearchRegistry() || Utilities.CheckUserPath(HudPath)) && !userSet) return;
            // Display a folder browser, ask the user to provide the tf/custom directory.
            Logger.Info("tf/custom directory is not set. Asking the user...");
            using (var browser = new FolderBrowserDialog
            {
                Description = Properties.Resources.info_path_browser,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            })
            {
                // Loop until the user provides a valid tf/custom directory, unless they cancel out.
                while (!browser.SelectedPath.EndsWith("tf\\custom"))
                    if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (browser.SelectedPath.EndsWith("tf\\custom"))
                        {
                            Settings.Default.hud_directory = browser.SelectedPath;
                            Settings.Default.Save();
                            HudPath = Settings.Default.hud_directory;
                            Logger.Info("tf/custom directory is set to: " + Settings.Default.hud_directory);
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

            // Check one more time if a valid directory has been set.
            if (Utilities.CheckUserPath(HudPath)) return;
            Logger.Info("tf/custom directory still not set. Exiting...");
            ShowMessageBox(MessageBoxImage.Warning, Utilities.GetLocalizedString("error_app_directory"));
            Application.Current.Shutdown();
        }

        /// <summary>
        ///     Display a message box with a message to the user and log it.
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
        ///     Check if the selected hud is installed in tf/custom.
        /// </summary>
        /// <returns>True if the selected hud is installed.</returns>
        public static bool CheckHudInstallation(HUD hud)
        {
            return hud != null &&
                MainWindow.HudPath != null &&
                Directory.Exists(MainWindow.HudPath) &&
                Utilities.CheckUserPath(MainWindow.HudPath) &&
                Directory.Exists($"{MainWindow.HudPath}\\{hud.Name}");
        }

        /// <summary>
        ///     Updates the local schema files to the latest version.
        /// </summary>
        /// <param name="silent">If true, the user will not be notified if there are no updates on startup.</param>
        public static async void CheckSchemaUpdates(bool silent = true)
        {
            // Check for HUD updates.
            Logger.Info("Checking for schema updates.");
            var restartRequired = await Update();
            if (restartRequired)
            {
                if (ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            }
            else
            {
                if (!silent)
                    ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update_none);
            }
        }

        /// <summary>
        ///     Synchronize the local HUD schema files with the latest versions on GitHub.
        /// </summary>
        /// <returns>Whether updates are available</returns>
        public static async Task<bool> Update()
        {
            try
            {
                var remoteFiles = (await Utilities.Fetch<GitJson[]>(Settings.Default.json_list)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();
                List<Task> downloads = new();

                foreach (var remoteFile in remoteFiles)
                {
                    var localFilePath = $"JSON\\{remoteFile.Name}";
                    bool newFile = false, fileChanged = false;

                    if (!File.Exists(localFilePath))
                        newFile = true;
                    else
                        fileChanged = remoteFile.SHA != Utilities.GitHash(localFilePath);

                    if (!newFile && !fileChanged) continue;
                    MainWindow.Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                    downloads.Add(Utilities.DownloadFile(remoteFile.Download, localFilePath));
                }

                // Remove HUD JSONs that aren't available online.
                foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
                {
                    if (remoteFiles.Count((x) => x.Name == localFile.Name) != 0) continue;
                    MainWindow.Logger.Info($"Deleting {localFile.Name}");
                    File.Delete(localFile.FullName);
                }

                await Task.WhenAll(downloads);
                return Convert.ToBoolean(downloads.Count);
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }
    }
}