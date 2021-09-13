using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoUpdaterDotNET;
using HUDEditor.Classes;
using HUDEditor.Properties;
using log4net;
using log4net.Config;
using TF2HUDEditor.Classes;
using WPFLocalizeExtension.Engine;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string HudSelection;
        public static string HudPath;
        private readonly List<(HUD, Border)> HudThumbnails = new();
        private bool DisplayUniqueHUDsOnly;
        private readonly INotifier _notifier;
        private readonly ILocalization _localization;
        private readonly IAppSettings _settings;
        private readonly VTF _vtf;
        private readonly IHUDUpdateChecker _hudUpdateChecker;
        private readonly HudDirectory _hudDirectory;
        private readonly ILog _logger;
        private readonly IUtilities _utilities;

        public MainWindow(
            ILog logger,
            IUtilities utilities,
            HudDirectory hudDirectory,
            INotifier notifier,
            ILocalization localization,
            IAppSettings settings,
            VTF vtf,
            IHUDUpdateChecker hudUpdateChecker) : base()
        {
            InitializeComponent();
            DataContext = this;

            _logger = logger;
            _utilities = utilities;
            _hudDirectory = hudDirectory;
            _notifier = notifier;
            _localization = localization;
            _settings = settings;
            _vtf = vtf;
            _hudUpdateChecker = hudUpdateChecker;
            Json = new Json(_logger, _utilities, _notifier, _localization, _settings, _vtf, _hudUpdateChecker);

            HudSelection = _settings.HudSelected;
            HudPath = _settings.HudDirectory;

            AddHUDs();
            SetupUI();
            CheckForUpdates();
        }

        public static Json Json { get; set; }

        /// <summary>
        ///     Add a HUD to the list of available HUDs.
        /// </summary>
        /// <param name="hud">HUd object to add to the list.</param>
        public void AddHUDToGridView(HUD hud)
        {
            _logger.Info($"Loading {hud.Name}");

            Border border = CreateBorder(hud);
            Label thumbnailIcon = CreateThumbnailIcon(hud);
            Image thumbnailImage = CreateThumbnailImage(hud);
            Label hudLabel = CreateHudLabel(hud);

            Grid hudIconContainer = BuildHudContainer(thumbnailIcon, thumbnailImage, hudLabel);

            border.Child = hudIconContainer;
            GridSelectHud.Children.Add(border);
            HudThumbnails.Add((hud, border));
        }

        private static Grid BuildHudContainer(Label thumbnailIcon, Image thumbnailImage, Label hudLabel)
        {
            var hudIconContainer = new Grid();
            hudIconContainer.RowDefinitions.Add(new RowDefinition());
            hudIconContainer.RowDefinitions.Add(new RowDefinition());
            hudIconContainer.Children.Add(thumbnailImage);
            hudIconContainer.Children.Add(thumbnailIcon);
            hudIconContainer.Children.Add(hudLabel);
            return hudIconContainer;
        }

        private static Label CreateHudLabel(HUD hud)
        {
            return new Label
            {
                Content = hud.Name,
                Style = (Style)Application.Current.Resources["HudListLabel"]
            };
        }

        private static Label CreateThumbnailIcon(HUD hud)
        {
            return new Label
            {
                Content = hud.Unique ? "B" : "",
                Style = (Style)Application.Current.Resources["HudListIcon"]
            };
        }

        private static Image CreateThumbnailImage(HUD hud)
        {
            var thumbnailImage = new Image
            {
                Source = hud.Thumbnail != null ? new BitmapImage(new Uri(hud.Thumbnail)) : null,
                Style = (Style)Application.Current.Resources["HudListImage"]
            };

            // Prevents image from looking grainy when resized
            RenderOptions.SetBitmapScalingMode(thumbnailImage, BitmapScalingMode.Fant);
            return thumbnailImage;
        }

        private Border CreateBorder(HUD hud)
        {
            var border = new Border
            {
                Style = (Style)Application.Current.Resources["HudListBorder"]
            };

            border.MouseEnter += (_, _) => border.BorderBrush = Brushes.Black;
            border.MouseLeave += (_, _) => border.BorderBrush = Brushes.LightGray;
            border.MouseUp += (_, _) =>
            {
                if (Json.HighlightedHUD == hud)
                {
                    Json.SelectedHUD = hud;
                }
                else
                {
                    Json.SelectedHUD = null;
                    Json.HighlightedHUD = hud;
                    HudInfo.ScrollToVerticalOffset(0);
                    _logger.Info($"Highlighting {hud.Name}");
                }
            };
            return border;
        }

        /// <summary>
        ///     Called when a new HUD has been selected from the list.
        /// </summary>
        /// <param name="hud">Selected HUD object.</param>
        public void SelectionChanged(object sender, HUD hud)
        {
            if (hud != null)
            {
                // Save the selected HUD.
                HudSelection = hud.Name;
                if (Json.SelectedHUDInstalled)
                    LblStatus.Content = string.Format(_localization.StatusInstalled, Json.SelectedHUD.Name);
                else if (Directory.Exists(HudPath))
                    LblStatus.Content = string.Format(_localization.StatusInstalledNot, Json.SelectedHUD.Name);
                else
                    LblStatus.Content = _localization.StatusPathNotSet;

                Application.Current.MainWindow.WindowState = hud.Maximize ? WindowState.Maximized : WindowState.Normal;
                _settings.HudSelected = hud.Name;
                _settings.Save();
            }
            else
            {
                HudSelection = string.Empty;
                LblStatus.Content = string.Empty;
            }

            SetPageView(hud);
        }

        private void AddHUDs()
        {
            foreach (var hud in Json.HUDList)
                AddHUDToGridView(hud);
        }

        private void SetupUI()
        {
            SetPageView(Json.SelectedHUD);
            SelectionChanged(Json, Json.GetHUDByName(_settings.HudSelected));
            _hudDirectory.Setup(HudPath);
            Json.SelectionChanged += SelectionChanged;
        }

        private void CheckForUpdates()
        {
            _logger.Info("Checking for app updates.");
            AutoUpdater.Start(_settings.AppUpdate);
        }

        #region PAGE_EVENTS

        /// <summary>
        ///     Update the page view to the selected HUD.
        /// </summary>
        private void SetPageView(HUD selection)
        {
            try
            {
                EditorContainer.Children.Clear();

                // If there's a HUD selection, generate the controls for that HUD's page.
                if (selection is null) return;

                _logger.Info($"Changing page view to: {selection.Name}.");
                EditorContainer.Children.Add(selection.GetControls());
                selection.PresetChanged += (_, _) =>
                {
                    EditorContainer.Children.Clear();
                    EditorContainer.Children.Add(selection.GetControls());
                };
            }
            catch (Exception e)
            {
                _notifier.ShowMessageBox(MessageBoxImage.Error, e.Message);
            }
        }

        /// <summary>
        ///     Check if the selected hud is installed in tf/custom.
        /// </summary>
        /// <returns>True if the selected hud is installed.</returns>
        public static bool CheckHudInstallation()
        {
            return Directory.Exists($"{HudPath}\\{HudSelection}");
        }

        #endregion

        #region CLICK_EVENTS

        private void TbSearchBox_TextChanged(object sender, RoutedEventArgs e)
        {
            foreach (var (hud, border) in HudThumbnails)
            {
                var searches = new[]
                {
                    hud.Name,
                    hud.Author,
                    hud.Description
                };

                var index = 0;
                var visibility = Visibility.Collapsed;
                while (visibility == Visibility.Collapsed && index < searches.Length)
                {
                    if (searches[index] != null && searches[index].ToLower().Contains(SearchBox.Text.ToLower()) && (!DisplayUniqueHUDsOnly || hud.Unique))
                        visibility = Visibility.Visible;
                    index++;
                }

                border.Visibility = visibility;
            }
        }

        private void UniqueHUDsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DisplayUniqueHUDsOnly = !DisplayUniqueHUDsOnly;

            if (DisplayUniqueHUDsOnly)
            {
                UniqueHUDsButton.Foreground = Brushes.SkyBlue;
                foreach (var (hud, border) in HudThumbnails)
                    border.Visibility = hud.Unique ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                UniqueHUDsButton.Foreground = Brushes.White;
                foreach (var (hud, border) in HudThumbnails)
                    border.Visibility = Visibility.Visible;
            }

            TbSearchBox_TextChanged(sender, e);
        }

        /// <summary>
        ///     Invoke HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                // Prevent switching HUD while installing to ensure that HighlightedHUD is the same as SelectedHUD at worker.RunWorkerCompleted
                BtnSwitch.IsEnabled = false;
                Json.SelectedHUD = Json.HighlightedHUD;
                _settings.HudSelected = HudSelection = Json.SelectedHUD.Name;

                // Force the user to set a directory before installing.
                if (!_utilities.CheckUserPath(HudPath))
                    _hudDirectory.Setup(HudPath, true);

                // Stop the process if Team Fortress 2 is still running.
                if (_utilities.IsGameRunning())
                {
                    _notifier.ShowMessageBox(MessageBoxImage.Warning, _utilities.GetLocalizedString(_localization.InfoGameRunning));
                    return;
                }

                var worker = new BackgroundWorker();
                worker.DoWork += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Step 1. Retrieve the HUD object, then download and extract it into the tf/custom directory.
                        _logger.Info($"Start installing {HudSelection}.");
                        Json.SelectedHUD.Update();

                        // Step 2. Clear tf/custom directory of other installed HUDs.
                        _logger.Info("Preparing directories for extraction.");
                        foreach (var x in Json.HUDList)
                            if (Directory.Exists(HudPath + "\\" + x.Name?.ToLowerInvariant()))
                                Directory.Delete(HudPath + "\\" + x.Name?.ToLowerInvariant(), true);

                        // Step 3. Record the name of the HUD inside the downloaded folder.
                        var tempFile = $"{AppDomain.CurrentDomain.BaseDirectory}temp.zip";
                        if (!File.Exists(tempFile)) tempFile = "temp.zip";
                        using var archive = ZipFile.OpenRead(tempFile);
                        var hudName = archive.Entries.FirstOrDefault(entry =>
                            entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase) &&
                            string.IsNullOrWhiteSpace(entry.Name));

                        // Step 4. Extract the downloaded HUD and rename it to match the schema.
                        _logger.Info($"Extracting {HudSelection} to: {HudPath}");
                        ZipFile.ExtractToDirectory(tempFile, HudPath);
                        Directory.Move($"{HudPath}\\{hudName}", $"{HudPath}\\{HudSelection}");

                        // Step 5. Update the page view.
                        if (string.IsNullOrWhiteSpace(HudSelection)) return;
                        Json.SelectedHUD.Settings.SaveSettings();
                        SetPageView(Json.SelectedHUD);
                        Json.SelectedHUD.ApplyCustomizations();
                    });
                };
                worker.RunWorkerCompleted += (_, _) =>
                {
                    LblStatus.Content = string.Format(_localization.StatusInstalledNow,
                        _settings.HudSelected, DateTime.Now);

                    // Update Install/Uninstall/Reset Buttons
                    Json.OnPropertyChanged("HighlightedHUDInstalled");

                    // Update Switch HUD Button
                    BtnSwitch.SetBinding(
                        IsEnabledProperty,
                        new Binding
                        {
                            Source = Json,
                            Path = new PropertyPath("SelectedHUD"),
                            Converter = new NullCheckConverter()
                        }
                    );
                    Json.OnPropertyChanged("SelectedHUD");

                    // Clean the application directory.
                    var tempPath = $"{Directory.GetCurrentDirectory()}\\temp.zip";
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                };
                worker.RunWorkerAsync();
            }
            catch (Exception e)
            {
                _notifier.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(_utilities.GetLocalizedString(_localization.ErrorHudInstall), HudSelection)} {e.Message}");
            }
        }

        /// <summary>
        ///     Invoke HUD deletion from the tf/custom directory.
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                // Check if the HUD is installed in a valid directory.
                if (!CheckHudInstallation()) return;

                // Stop the process if Team Fortress 2 is still running.
                if (_utilities.IsGameRunning())
                {
                    _notifier.ShowMessageBox(MessageBoxImage.Warning, _utilities.GetLocalizedString(_localization.InfoGameRunning));
                    return;
                }

                // Remove the HUD from the tf/custom directory.
                _logger.Info($"Start uninstalling {HudSelection}.");
                _logger.Info($"Removing {HudSelection} from: {HudPath}");
                Directory.Delete(HudPath + $"\\{HudSelection}", true);
                Json.OnPropertyChanged("HighlightedHUD");
                Json.OnPropertyChanged("HighlightedHUDInstalled");
            }
            catch (Exception e)
            {
                _notifier.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(_utilities.GetLocalizedString(_localization.ErrorHudUninstall), HudSelection)} {e.Message}");
            }
        }

        /// <summary>
        ///     Save and apply user settings to the HUD files.
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var hudObject = Json.GetHUDByName(_settings.HudSelected);
            if (Process.GetProcessesByName("hl2").Any() && hudObject.DirtyControls.Count > 0)
            {
                var message = hudObject.DirtyControls.Aggregate(_localization.InfoGameRestart, (current, control) => current + $"\n - {control}");
                if (_notifier.ShowMessageBox(MessageBoxImage.Question, message) != MessageBoxResult.OK) return;
            }

            _logger.Info("Start applying settings.");
            var worker = new BackgroundWorker();
            worker.DoWork += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(HudSelection)) return;
                    var selection = Json.GetHUDByName(_settings.HudSelected);
                    selection.Settings.SaveSettings();
                    selection.ApplyCustomizations();
                    selection.DirtyControls.Clear();
                    _logger.Info("Done applying settings.");
                });
            };
            worker.RunWorkerCompleted += (_, _) =>
            {
                LblStatus.Content = string.Format(_localization.StatusApplied, hudObject.Name, DateTime.Now);
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Reset user settings for the selected HUD to default.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to reset before doing so.
            if (_notifier.ShowMessageBox(MessageBoxImage.Question, _localization.InfoHudReset, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _logger.Info("Start resetting settings.");
            var selection = Json.SelectedHUD;
            selection.ResetAll();
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();
            LblStatus.Content = string.Format(_localization.StatusReset, selection.Name, DateTime.Now);
            _logger.Info("Done resetting settings.");
        }

        /// <summary>
        ///     Return to the HUD selection page.
        /// </summary>
        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            _logger.Info("Changing page view to: main menu.");
            EditorContainer.Children.Clear();
            Json.HighlightedHUD = null;
            Json.SelectedHUD = null;
        }

        private void BtnSetDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            _logger.Info("Attempting to change the 'tf/custom' directory.");
            _hudDirectory.Setup(HudPath, true);
        }

        /// <summary>
        ///     Add a HUD from folder to the shared HUDs list.
        /// </summary>
        private void BtnAddSharedHUD_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_notifier.ShowMessageBox(MessageBoxImage.Information, _localization.InfoAddHud, MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes) return;

                var browser = new FolderBrowserDialog
                {
                    SelectedPath = HudPath + "\\"
                };
                if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                AddHUDToGridView(Json.Add(browser.SelectedPath));
            }
            catch (Exception error)
            {
                _notifier.ShowMessageBox(MessageBoxImage.Error, error.Message);
            }
        }

        /// <summary>
        ///     Opens the issue tracker for the editor.
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(_settings.AppTracker);
        }

        /// <summary>
        ///     Opens the project documentation site.
        /// </summary>
        private void BtnDocumentation_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(_settings.AppDocs);
        }

        /// <summary>
        ///     Updates the local schema files to the latest version.
        /// </summary>
        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            // Check for HUD updates.
            _logger.Info("Checking for schema updates.");
            Json.UpdateAsync().ContinueWith(restartRequired =>
            {
                if (!restartRequired.Result)
                {
                    _notifier.ShowMessageBox(MessageBoxImage.Information, _localization.InfoHudUpdateNone);
                    return;
                }

                if (_notifier.ShowMessageBox(MessageBoxImage.Information, _localization.InfoHudUpdate, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                Json.Update(true);
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            });
        }

        private void BtnGitHub_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(Json.HighlightedHUD.GitHubUrl);
        }

        private void BtnHuds_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(Json.HighlightedHUD.HudsTfUrl);
        }

        private void BtnDiscord_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(Json.HighlightedHUD.DiscordUrl);
        }

        private void BtnSteam_OnClick(object sender, RoutedEventArgs e)
        {
            _utilities.OpenWebpage(Json.HighlightedHUD.SteamUrl);
        }

        /// <summary>
        ///     Updates localization to the selected language.
        /// </summary>
        private void BtnLocalize_OnClick(object sender, RoutedEventArgs e)
        {
            if (btnLocalizeFR.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("fr-FR");
            else if (btnLocalizeRU.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("ru-RU");
            else
                LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");

            // Save language preference to user settings.
            _settings.UserLanguage = LocalizeDictionary.Instance.Culture.ToString();
            _settings.Save();
        }


        private void BtnCustomize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Json.HighlightedHUD is null) return;
            EditorContainer.Children.Clear();
            Json.SelectedHUD = Json.HighlightedHUD;
            _settings.HudSelected = Json.SelectedHUD.Name;
            _settings.Save();
            SetPageView(Json.GetHUDByName(_settings.HudSelected));
        }

        #endregion
    }
}