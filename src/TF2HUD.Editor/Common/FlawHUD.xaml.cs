using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using TF2HUD.Editor.Properties;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace TF2HUD.Editor.Common
{
    /// <summary>
    ///     Interaction logic for FlawHUD.xaml
    /// </summary>
    public partial class FlawHUD : UserControl
    {
        private static readonly string _hudPath = Properties.Resources.dir_custom;

        public FlawHUD()
        {
            InitializeComponent();
            SetupDirectory();
            ReloadHudSettings();
            SetCrosshairControls();
        }

        /// <summary>
        ///     Set the tf/custom directory if not already set
        /// </summary>
        private void SetupDirectory(bool userSet = false)
        {
            if (!MainWindow.SearchRegistry() && !CheckUserPath() || userSet)
                //Logger.Info("Setting the tf/custom directory. Opening folder browser, asking the user.");
                //using (var browser = new FolderBrowserDialog
                //{ Description = TF2HUD.Editor.Properties.Resources.info_folder_browser, ShowNewFolderButton = true })
                //{
                //    while (!browser.SelectedPath.Contains("tf\\custom"))
                //        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                //            browser.SelectedPath.Contains("tf\\custom"))
                //        {
                //            var settings = Settings.Default;
                //            settings.hud_directory = browser.SelectedPath;
                //            settings.Save();
                //            //LblStatus.Content = settings.hud_directory;
                //            //MainWindow.Logger.Info("Directory has been set to " + LblStatus.Content);
                //        }
                //        else
                //        {
                //            break;
                //        }
                //}

                if (!CheckUserPath())
                {
                    MainWindow.Logger.Error("Unable to set the tf/custom directory. Exiting.");
                    MessageBox.Show(Properties.Resources.error_app_directory,
                        Properties.Resources.error_app_directory_title, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }

            CleanDirectory();
            SetFormControls();
        }

        /// <summary>
        ///     Cleans up the tf/custom and installer directories
        /// </summary>
        private static void CleanDirectory()
        {
            MainWindow.Logger.Info("Cleaning-up FlawHUD directories...");

            // Clean the application directory
            if (File.Exists(Directory.GetCurrentDirectory() + "\\flawhud.zip"))
                File.Delete(Directory.GetCurrentDirectory() + "\\flawhud.zip");

            // Clean the tf/custom directory
            var hudDirectory = Directory.Exists(_hudPath + "\\flawhud-master")
                ? _hudPath + "\\flawhud-master"
                : string.Empty;

            if (!string.IsNullOrEmpty(hudDirectory))
            {
                // Remove the previous backup if found.
                if (File.Exists(_hudPath + "\\flawhud-backup.zip"))
                    File.Delete(_hudPath + "\\flawhud-backup.zip");

                MainWindow.Logger.Info("Found a FlawHUD installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, _hudPath + "\\flawhud-backup.zip");
                Directory.Delete(hudDirectory, true);
                MessageBox.Show(Properties.Resources.info_create_backup, Properties.Resources.info_create_backup_title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            MainWindow.Logger.Info("Cleaning-up FlawHUD directories...Done!");
        }

        /// <summary>
        ///     Check if FlawHUD is installed in the tf/custom directory
        /// </summary>
        public static bool CheckHudPath()
        {
            return Directory.Exists(_hudPath + "\\flawhud");
        }

        /// <summary>
        ///     Check if user's directory setting is valid
        /// </summary>
        public static bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(_hudPath) && _hudPath.Contains("tf\\custom");
        }

        /// <summary>
        ///     Update the installer controls like labels and buttons
        /// </summary>
        private void SetFormControls()
        {
            if (Directory.Exists(_hudPath) && CheckUserPath())
            {
                var isInstalled = CheckHudPath();
                BtnStart.IsEnabled = true;
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Reinstall" : "Install";
                BtnSave.IsEnabled = isInstalled;
                BtnUninstall.IsEnabled = isInstalled;
                //LblStatus.Content = $"FlawHUD is {(!isInstalled ? "not " : "")}installed...";
                Settings.Default.Save();
            }
            else
            {
                BtnStart.IsEnabled = false;
                BtnInstall.IsEnabled = false;
                BtnInstall.Content = "Set Directory";
                BtnSave.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                //LblStatus.Content = "Directory is not set...";
            }
        }

        /// <summary>
        ///     Disables certain crosshair options if rotating crosshair is enabled
        /// </summary>
        private void SetCrosshairControls()
        {
            CbXHairHitmarker.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairRotate.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //IntXHairSize.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
        }

        #region CLICK_EVENTS

        /// <summary>
        ///     Installs FlawHUD to the user's tf/custom folder
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckHudPath())
                {
                    MainWindow.Logger.Info("Opening Directory Browser...");
                    SetupDirectory(true);
                    return;
                }

                if (!MainWindow.CheckGameStatus()) return;
                MainWindow.Logger.Info("Installing FlawHUD...");
                MainWindow.Logger.Info("Downloading the latest FlawHUD...");
                MainWindow.DownloadHud(Properties.Resources.download_flawhud);
                MainWindow.Logger.Info("Downloading the latest FlawHUD...Done!");
                MainWindow.Logger.Info("Extracting downloaded FlawHUD to " + _hudPath);
                ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\flawhud.zip", _hudPath);
                if (Directory.Exists(_hudPath + "\\flawhud"))
                    Directory.Delete(_hudPath + "\\flawhud", true);
                if (Directory.Exists(_hudPath + "\\flawhud-master"))
                    Directory.Move(_hudPath + "\\flawhud-master", _hudPath + "\\flawhud");
                MainWindow.Logger.Info("Extracting downloaded FlawHUD...Done!");
                CleanDirectory();
                SaveHudSettings();
                ApplyHudSettings();
                SetFormControls();
                MainWindow.Logger.Info("Installing FlawHUD...Done!");
                MessageBox.Show(Properties.Resources.info_install_complete_desc,
                    Properties.Resources.info_install_complete, MessageBoxButton.OK, MessageBoxImage.Information);

                //var worker = new BackgroundWorker();
                //worker.DoWork += (_, _) =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        DownloadHud();
                //        SaveHudSettings();
                //        ApplyHudSettings();
                //        SetFormControls();
                //    });
                //};
                //worker.RunWorkerCompleted += (_, _) =>
                //{
                //    //LblNews.Content = "Installation finished at " + DateTime.Now;
                //    Logger.Info("Installing FlawHUD...Done!");
                //    MessageBox.Show(TF2HUD.Editor.Properties.Resources.info_install_complete_desc,
                //        TF2HUD.Editor.Properties.Resources.info_install_complete, MessageBoxButton.OK, MessageBoxImage.Information);
                //};
                //worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Installing FlawHUD.", Properties.Resources.error_app_install, ex.Message);
            }
        }

        /// <summary>
        ///     Removes FlawHUD from the user's tf/custom folder
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MainWindow.CheckGameStatus()) return;
                MainWindow.Logger.Info("Uninstalling FlawHUD...");
                if (!CheckHudPath()) return;
                Directory.Delete(_hudPath + "\\flawhud", true);
                //LblNews.Content = "Uninstalled FlawHUD at " + DateTime.Now;
                SetupDirectory();
                MainWindow.Logger.Info("Uninstalling FlawHUD...Done!");
                MessageBox.Show(Properties.Resources.info_uninstall_complete_desc,
                    Properties.Resources.info_uninstall_complete, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Uninstalling FlawHUD.", Properties.Resources.error_app_uninstall,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Saves then applies the FlawHUD settings
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveHudSettings();
            ApplyHudSettings();
            //var worker = new BackgroundWorker();
            //worker.DoWork += (_, _) =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        SaveHudSettings();
            //        ApplyHudSettings();
            //    });
            //};
            ////worker.RunWorkerCompleted();
            //worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Resets the FlawHUD settings to the default
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetHudSettings();
        }

        /// <summary>
        ///     Launches Team Fortress 2 through Steam
        /// </summary>
        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Logger.Info("Launching Team Fortress 2...");
                Process.Start("steam://rungameid/440");
            }
            catch (Exception ex)
            {
                MainWindow.Logger.Error(ex.Message);
            }
        }

        /// <summary>
        ///     Opens the GitHub issue tracker in a web browser
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Logger.Info("Opening Issue Tracker...");
                Process.Start(Properties.Resources.app_tracker);
            }
            catch
            {
                var url = Properties.Resources.app_tracker;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Disables certain crosshair options if rotating crosshair is enabled
        /// </summary>
        private void CbXHairEnable_OnClick(object sender, RoutedEventArgs e)
        {
            SetCrosshairControls();
        }

        private void CbLowerPlayerStats_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbLowerPlayerStats.IsChecked != null)
                CbAlternatePlayerStats.IsEnabled = !(bool) CbLowerPlayerStats.IsChecked;
        }

        private void CbAlternatePlayerStats_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbAlternatePlayerStats.IsChecked != null)
                CbLowerPlayerStats.IsEnabled = !(bool) CbAlternatePlayerStats.IsChecked;
        }

        #endregion CLICK_EVENTS

        #region SAVE_LOAD

        /// <summary>
        ///     Save user settings to the file
        /// </summary>
        private void SaveHudSettings()
        {
            try
            {
                MainWindow.Logger.Info("Saving HUD Settings...");
                var settings = flawhud.Default;
                settings.color_health_buff = CpHealthBuffed.SelectedColor?.ToString();
                settings.color_health_low = CpHealthLow.SelectedColor?.ToString();
                settings.color_ammo_low = CpAmmoLow.SelectedColor?.ToString();
                settings.color_uber_bar = CpUberBarColor.SelectedColor?.ToString();
                settings.color_uber_full = CpUberFullColor.SelectedColor?.ToString();
                settings.color_xhair_normal = CpXHairColor.SelectedColor?.ToString();
                settings.color_xhair_pulse = CpXHairPulse.SelectedColor?.ToString();
                settings.color_target_health = CpTargetHealth.SelectedColor?.ToString();
                settings.color_target_damage = CpTargetDamage.SelectedColor?.ToString();
                //settings.val_xhair_size = IntXHairSize.Value ?? 18;
                settings.val_xhair_style = CbXHairStyle.SelectedIndex;
                settings.val_xhair_effect = CbXHairEffect.SelectedIndex;
                settings.toggle_xhair_enable = CbXHairEnable.IsChecked ?? false;
                settings.toggle_xhair_pulse = CbXHairHitmarker.IsChecked ?? false;
                settings.toggle_xhair_rotate = CbXHairRotate.IsChecked ?? false;
                settings.toggle_disguise_image = CbDisguiseImage.IsChecked ?? false;
                settings.toggle_stock_backgrounds = CbDefaultBg.IsChecked ?? false;
                settings.toggle_menu_images = CbMenuImages.IsChecked ?? false;
                settings.toggle_transparent_viewmodels = CbTransparentViewmodel.IsChecked ?? false;
                settings.toggle_code_fonts = CbCodeProFonts.IsChecked ?? false;
                settings.toggle_lower_stats = CbLowerPlayerStats.IsChecked ?? false;
                settings.toggle_alt_stats = CbAlternatePlayerStats.IsChecked ?? false;
                settings.val_health_style = CbHealthStyle.SelectedIndex;
                //settings.val_killfeed_rows = IntKillFeedRows.Value ?? 5;
                settings.Save();
                MainWindow.Logger.Info("Saving HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Saving HUD Settings.", Properties.Resources.error_app_save, ex.Message);
            }
        }

        /// <summary>
        ///     Load GUI with user settings from the file
        /// </summary>
        private void ReloadHudSettings()
        {
            try
            {
                MainWindow.Logger.Info("Loading HUD Settings...");
                var settings = flawhud.Default;
                var cc = new ColorConverter();
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_buff);
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_low);
                CpAmmoLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_low);
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_bar);
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_full);
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_normal);
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_pulse);
                CpTargetHealth.SelectedColor = (Color) cc.ConvertFrom(settings.color_target_health);
                CpTargetDamage.SelectedColor = (Color) cc.ConvertFrom(settings.color_target_damage);
                //IntXHairSize.Value = settings.val_xhair_size;
                CbXHairStyle.SelectedIndex = settings.val_xhair_style;
                CbXHairEffect.SelectedIndex = settings.val_xhair_effect;
                CbXHairEnable.IsChecked = settings.toggle_xhair_enable;
                CbXHairHitmarker.IsChecked = settings.toggle_xhair_pulse;
                CbXHairRotate.IsChecked = settings.toggle_xhair_rotate;
                CbDisguiseImage.IsChecked = settings.toggle_disguise_image;
                CbDefaultBg.IsChecked = settings.toggle_stock_backgrounds;
                CbMenuImages.IsChecked = settings.toggle_menu_images;
                CbTransparentViewmodel.IsChecked = settings.toggle_transparent_viewmodels;
                CbCodeProFonts.IsChecked = settings.toggle_code_fonts;
                CbLowerPlayerStats.IsChecked = settings.toggle_lower_stats;
                CbAlternatePlayerStats.IsChecked = settings.toggle_alt_stats;
                CbHealthStyle.SelectedIndex = settings.val_health_style;
                //IntKillFeedRows.Value = settings.val_killfeed_rows;
                MainWindow.Logger.Info("Loading HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Loading HUD Settings.", Properties.Resources.error_app_load, ex.Message);
            }
        }

        /// <summary>
        ///     Reset user settings to their default values
        /// </summary>
        private void ResetHudSettings()
        {
            try
            {
                MainWindow.Logger.Info("Resetting HUD Settings...");
                var cc = new ColorConverter();
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom("#BE1414");
                CpAmmoLow.SelectedColor = (Color) cc.ConvertFrom("#BE1414");
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom("#F2F2F2");
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom("#FF0000");
                CpTargetHealth.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpTargetDamage.SelectedColor = (Color) cc.ConvertFrom("#FFFF00");
                //IntXHairSize.Value = 18;
                CbXHairStyle.SelectedIndex = 24;
                CbXHairEffect.SelectedIndex = 0;
                CbXHairEnable.IsChecked = false;
                CbXHairHitmarker.IsChecked = true;
                CbXHairRotate.IsChecked = false;
                CbDisguiseImage.IsChecked = true;
                CbDefaultBg.IsChecked = false;
                CbMenuImages.IsChecked = true;
                CbTransparentViewmodel.IsChecked = false;
                CbCodeProFonts.IsChecked = false;
                CbLowerPlayerStats.IsChecked = false;
                CbAlternatePlayerStats.IsChecked = false;
                CbHealthStyle.SelectedIndex = 0;
                //IntKillFeedRows.Value = 5;
                SetCrosshairControls();
                //LblNews.Content = "Settings Reset at " + DateTime.Now;
                MainWindow.Logger.Info("Resetting HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Resetting HUD Settings.", Properties.Resources.error_app_reset,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Apply user settings to FlawHUD files
        /// </summary>
        private void ApplyHudSettings()
        {
            MainWindow.Logger.Info("Applying HUD Settings...");
            if (!MainMenuBackground()) return;
            if (!DisguiseImage()) return;
            if (!MainMenuClassImage()) return;
            //if (!Crosshair(CbXHairStyle.SelectedValue.ToString(), IntXHairSize.Value,
            //    CbXHairEffect.SelectedValue.ToString())) return;
            if (!CrosshairPulse()) return;
            if (!CrosshairRotate()) return;
            if (!Colors()) return;
            if (!TransparentViewmodels()) return;
            if (!CodeProFonts()) return;
            if (!HealthStyle()) return;
            if (!KillFeedRows()) return;
            if (!LowerPlayerStats()) return;
            if (!AlternatePlayerStats()) return;
            //LblNews.Content = "Settings Saved at " + DateTime.Now;
            MainWindow.Logger.Info("Resetting HUD Settings...Done!");
        }

        #endregion SAVE_LOAD

        #region CONTROLLER

        /// <summary>
        ///     Update the client scheme colors.
        /// </summary>
        public bool Colors()
        {
            try
            {
                MainWindow.Logger.Info("Updating the color client scheme.");
                var file = _hudPath + Properties.Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[FindIndex(lines, "\"Overheal\"")] =
                    $"\t\t\"Overheal\"\t\t\t\t\t\"{RgbConverter(flawhud.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "OverhealPulse")] =
                    $"\t\t\"OverhealPulse\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_health_buff, true)}\"";
                lines[FindIndex(lines, "\"LowHealth\"")] =
                    $"\t\t\"LowHealth\"\t\t\t\t\t\"{RgbConverter(flawhud.Default.color_health_low)}\"";
                lines[FindIndex(lines, "LowHealthPulse")] =
                    $"\t\t\"LowHealthPulse\"\t\t\t\"{RgbConverter(flawhud.Default.color_health_low, true)}\"";
                // Ammo
                lines[FindIndex(lines, "\"LowAmmo\"")] =
                    $"\t\t\"LowAmmo\"\t\t\t\t\t\"{RgbConverter(flawhud.Default.color_ammo_low)}\"";
                lines[FindIndex(lines, "LowAmmoPulse")] =
                    $"\t\t\"LowAmmoPulse\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_ammo_low, true)}\"";
                // Misc
                lines[FindIndex(lines, "\"PositiveValue\"")] =
                    $"\t\t\"PositiveValue\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "NegativeValue")] =
                    $"\t\t\"NegativeValue\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_health_low, true)}\"";
                lines[FindIndex(lines, "\"TargetHealth\"")] =
                    $"\t\t\"TargetHealth\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_target_health)}\"";
                lines[FindIndex(lines, "TargetDamage")] =
                    $"\t\t\"TargetDamage\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_target_damage)}\"";
                // Crosshair
                lines[FindIndex(lines, "\"Crosshair\"")] =
                    $"\t\t\"Crosshair\"\t\t\t\t\t\"{RgbConverter(flawhud.Default.color_xhair_normal)}\"";
                lines[FindIndex(lines, "CrosshairDamage")] =
                    $"\t\t\"CrosshairDamage\"\t\t\t\"{RgbConverter(flawhud.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[FindIndex(lines, "UberCharged1")] =
                    $"\t\t\"UberCharged1\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_uber_full)}\"";
                lines[FindIndex(lines, "UberCharged2")] =
                    $"\t\t\"UberCharged2\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_uber_full, pulse: true)}\"";
                lines[FindIndex(lines, "UberCharging")] =
                    $"\t\t\"UberCharging\"\t\t\t\t\"{RgbConverter(flawhud.Default.color_uber_bar)}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating colors.", Properties.Resources.error_set_colors,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the health and ammo colors to be displayed on text instead of a panel.
        /// </summary>
        public bool ColorText(bool colorText = false)
        {
            try
            {
                MainWindow.Logger.Info("Setting player health color style.");
                var file = _hudPath + Properties.Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                // Panels
                CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", !colorText);
                CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", !colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoBG", !colorText);
                // Text
                CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "PlayerStatusHealthValue", colorText);
                CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "PlayerStatusHealthValue", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInClip", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInReserve", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoNoClip", colorText);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating player health color style.",
                    Properties.Resources.error_set_colors,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the crosshair style, position and effect.
        /// </summary>
        public bool Crosshair(string style, int? size, string effect)
        {
            try
            {
                MainWindow.Logger.Info("Updating crosshair settings.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "CustomCrosshair");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"labelText\"", start)] = "\t\t\"labelText\"\t\t\t\"<\"";
                lines[FindIndex(lines, "font", start)] = "\t\t\"font\"\t\t\t\t\"Size:18 | Outline:OFF\"";
                File.WriteAllLines(file, lines);

                if (flawhud.Default.toggle_xhair_rotate) return true;
                if (!flawhud.Default.toggle_xhair_enable) return true;
                var strEffect = effect != "None" ? $"{effect}:ON" : "Outline:OFF";
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"labelText\"", start)] = $"\t\t\"labelText\"\t\t\t\"{style}\"";
                lines[FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"Size:{size} | {strEffect}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating crosshair settings.", Properties.Resources.error_set_xhair,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the crosshair hitmarker.
        /// </summary>
        public bool CrosshairPulse()
        {
            try
            {
                MainWindow.Logger.Info("Toggling crosshair hitmarker.");
                var file = _hudPath + Properties.Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DamagedPlayer");
                var index1 = FindIndex(lines, "StopEvent", start);
                var index2 = FindIndex(lines, "RunEvent", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_xhair_pulse) return true;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling crosshair hitmarker.",
                    Properties.Resources.error_set_xhair_pulse,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the rotating crosshair.
        /// </summary>
        public bool CrosshairRotate()
        {
            try
            {
                MainWindow.Logger.Info("Toggling rotating crosshairs.");

                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_xhair_enable) return true;
                if (!flawhud.Default.toggle_xhair_rotate) return true;
                start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling rotating crosshairs.", Properties.Resources.error_set_xhair,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the visibility of the Spy's disguise image.
        /// </summary>
        public bool DisguiseImage()
        {
            try
            {
                MainWindow.Logger.Info("Toggling the Spy's disguise image.");
                var file = _hudPath + Properties.Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudSpyDisguiseFadeIn");
                var index1 = FindIndex(lines, "RunEvent", start);
                var index2 = FindIndex(lines, "Animate", start);
                start = FindIndex(lines, "HudSpyDisguiseFadeOut");
                var index3 = FindIndex(lines, "RunEvent", start);
                var index4 = FindIndex(lines, "Animate", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                lines[index3] = CommentOutTextLine(lines[index3]);
                lines[index4] = CommentOutTextLine(lines[index4]);
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_disguise_image) return true;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                lines[index3] = lines[index3].Replace("//", string.Empty);
                lines[index4] = lines[index4].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling the Spy's disguise image.",
                    Properties.Resources.error_set_spy_disguise_image, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the custom main menu backgrounds.
        /// </summary>
        public bool MainMenuBackground()
        {
            try
            {
                MainWindow.Logger.Info("Toggling custom main menu backgrounds.");
                var directory = new DirectoryInfo(_hudPath + Properties.Resources.dir_console);
                var chapterbackgrounds = _hudPath + Properties.Resources.file_chapterbackgrounds;
                var chapterbackgroundsTemp = chapterbackgrounds.Replace(".txt", ".file");
                var menu = _hudPath + Properties.Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(menu);
                var start = FindIndex(lines, "Background");
                var index1 = FindIndex(lines, "image", FindIndex(lines, "if_halloween", start));
                var index2 = FindIndex(lines, "image", FindIndex(lines, "if_christmas", start));

                if (flawhud.Default.toggle_stock_backgrounds)
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("upward", "off"));
                    if (File.Exists(chapterbackgrounds))
                        File.Move(chapterbackgrounds, chapterbackgroundsTemp);
                }
                else
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("off", "upward"));
                    if (File.Exists(chapterbackgroundsTemp))
                        File.Move(chapterbackgroundsTemp, chapterbackgrounds);
                }

                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(menu, lines);
                if (flawhud.Default.toggle_stock_backgrounds) return true;

                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                File.WriteAllLines(menu, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling custom main menu backgrounds.",
                    Properties.Resources.error_set_menu_backgrounds,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the visibility of the main menu class images.
        /// </summary>
        public bool MainMenuClassImage()
        {
            try
            {
                MainWindow.Logger.Info("Toggling main menu class images.");
                var file = _hudPath + Properties.Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "TFCharacterImage");
                var value = flawhud.Default.toggle_menu_images ? "-80" : "9999";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling main menu class images.",
                    Properties.Resources.error_set_menu_class_image,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the weapon viewmodel transparency.
        /// </summary>
        public bool TransparentViewmodels()
        {
            try
            {
                MainWindow.Logger.Info("Toggling transparent viewmodels.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = FindIndex(lines, "visible", start);
                var index2 = FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_transparent_viewmodels) return true;
                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(_hudPath + "\\flawhud\\cfg"))
                    Directory.CreateDirectory(_hudPath + "\\flawhud\\cfg");
                if (File.Exists(_hudPath + Properties.Resources.file_cfg))
                    File.Delete(_hudPath + Properties.Resources.file_cfg);
                File.Copy(Directory.GetCurrentDirectory() + "\\hud.cfg", _hudPath + Properties.Resources.file_cfg);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling transparent viewmodels.",
                    Properties.Resources.error_set_transparent_viewmodels, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Update the health indicator to use the cross style.
        /// </summary>
        public bool HealthStyle()
        {
            try
            {
                MainWindow.Logger.Info("Setting player health style.");
                var file = _hudPath + Properties.Resources.file_playerhealth;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"PlayerStatusHealthBonusImage\"");
                var index = FindIndex(lines, "image", start);
                lines[index] = "\t\t\"image\"\t\t\t\"\"";

                ColorText(flawhud.Default.val_health_style == 1);

                if (flawhud.Default.val_health_style == 2)
                {
                    lines[index] = "\t\t\"image\"\t\t\t\"../hud/health_over_bg\"";
                    File.WriteAllLines(file, lines);

                    file = _hudPath + Properties.Resources.file_hudanimations;
                    lines = File.ReadAllLines(file);
                    CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", false);
                    CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", false);
                }

                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting player health style.", Properties.Resources.error_set_colors,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle to a Code Pro font instead of the default.
        /// </summary>
        public bool CodeProFonts()
        {
            try
            {
                MainWindow.Logger.Info("Setting to the preferred font.");
                var file = _hudPath + Properties.Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = flawhud.Default.toggle_code_fonts ? "clientscheme_fonts_pro" : "clientscheme_fonts";
                lines[FindIndex(lines, "clientscheme_fonts")] =
                    $"#base \"scheme/{value}.res\"	// Change to fonts_pro.res for Code Pro fonts";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting to the preferred font.",
                    Properties.Resources.error_set_fonts,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the number of rows shown on the killfeed.
        /// </summary>
        public bool KillFeedRows()
        {
            try
            {
                MainWindow.Logger.Info("Setting the killfeed row count.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudDeathNotice");
                var value = flawhud.Default.val_killfeed_rows;
                lines[FindIndex(lines, "MaxDeathNotices", start)] = $"\t\t\"MaxDeathNotices\"\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting the killfeed row count.",
                    Properties.Resources.error_set_menu_class_image,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Lowers the player health and ammo.
        /// </summary>
        public bool LowerPlayerStats()
        {
            try
            {
                MainWindow.Logger.Info("Updating player health and ammo positions.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudWeaponAmmo");
                var value = flawhud.Default.toggle_lower_stats ? "r83" : "c93";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMannVsMachineStatus");
                value = flawhud.Default.toggle_lower_stats ? "-55" : "0";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CHealthAccountPanel");
                value = flawhud.Default.toggle_lower_stats ? "r150" : "267";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CSecondaryTargetID");
                value = flawhud.Default.toggle_lower_stats ? "325" : "355";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMenuSpyDisguise");
                value = flawhud.Default.toggle_lower_stats ? "c60" : "c130";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudSpellMenu");
                value = flawhud.Default.toggle_lower_stats ? "c-270" : "c-210";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "\"DamageAccountValue\"");
                value = flawhud.Default.toggle_lower_stats ? "r105" : "0";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_lower_stats ? "r108" : "c68";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                SetItemEffectPosition(string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, ""));
                SetItemEffectPosition(string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, "_cleaver"),
                    Positions.Middle);
                SetItemEffectPosition(
                    string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, "_sodapopper"),
                    Positions.Top);
                SetItemEffectPosition(_hudPath + Properties.Resources.dir_resource_ui + "\\huddemomancharge.res",
                    Positions.Middle,
                    "ChargeMeter");
                SetItemEffectPosition(_hudPath + Properties.Resources.dir_resource_ui + "\\huddemomanpipes.res",
                    Positions.Default,
                    "PipesPresentPanel");
                SetItemEffectPosition(_hudPath + Properties.Resources.dir_resource_ui + "\\hudrocketpack.res",
                    Positions.Middle);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating player health and ammo positions.",
                    Properties.Resources.error_set_lower_player_stats,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Repositions the player health and ammo.
        /// </summary>
        public bool AlternatePlayerStats()
        {
            try
            {
                // Skip if the player already has "Lowered Player Stats" enabled.
                if (flawhud.Default.toggle_lower_stats) return true;
                MainWindow.Logger.Info("Repositioning player health and ammo.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudWeaponAmmo");
                var value = flawhud.Default.toggle_alt_stats ? "r110" : "c90";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r50" : "c93";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMedicCharge");
                value = flawhud.Default.toggle_alt_stats ? "c60" : "c38";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CHealthAccountPanel");
                value = flawhud.Default.toggle_alt_stats ? "113" : "c-180";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r90" : "267";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "DisguiseStatus");
                value = flawhud.Default.toggle_alt_stats ? "115" : "100";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r62" : "r38";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CMainTargetID");
                value = flawhud.Default.toggle_alt_stats ? "r200" : "265";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudAchievementTracker");
                value = flawhud.Default.toggle_alt_stats ? "135" : "335";
                lines[FindIndex(lines, "NormalY", start)] = $"\t\t\"NormalY\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "9999" : "335";
                lines[FindIndex(lines, "EngineerY", start)] = $"\t\t\"EngineerY\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "9999" : "95";
                lines[FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_alt_stats ? "137" : "c-120";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r47" : "c70";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "\"DamageAccountValue\"");
                value = flawhud.Default.toggle_alt_stats ? "137" : "c-120";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r47" : "c70";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_alt_stats ? "10" : "c-190";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r75" : "c68";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_playerclass;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "PlayerStatusClassImage");
                value = flawhud.Default.toggle_alt_stats ? "r125" : "r75";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                start = FindIndex(lines, "classmodelpanel");
                value = flawhud.Default.toggle_alt_stats ? "r230" : "r200";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "180" : "200";
                lines[FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, string.Empty);
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r110" : "c-60";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r65" : "c120";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "ItemEffectMeterLabel");
                value = flawhud.Default.toggle_alt_stats ? "100" : "120";
                lines[FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "\"ItemEffectMeter\"");
                value = flawhud.Default.toggle_alt_stats ? "100" : "120";
                lines[FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, "_cleaver");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r85" : "c110";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, "_sodapopper");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r75" : "c100";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Properties.Resources.file_itemeffectmeter, "_killstreak");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "115" : "2";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r33" : "r28";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_hudanimations;
                File.WriteAllText(file,
                    flawhud.Default.toggle_alt_stats
                        ? File.ReadAllText(file).Replace("Blank", "HudBlack")
                        : File.ReadAllText(file).Replace("HudBlack", "Blank"));
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error repositioning player health and ammo.",
                    Properties.Resources.error_set_lower_player_stats,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Retrieves the index of where a given value was found in a string array.
        /// </summary>
        public static int FindIndex(string[] array, string value, int skip = 0)
        {
            var list = array.Skip(skip);
            var index = list.Select((v, i) => new {Index = i, Value = v}) // Pair up values and indexes
                .Where(p => p.Value.Contains(value)) // Do the filtering
                .Select(p => p.Index); // Keep the index and drop the value
            return index.First() + skip;
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public static string CommentOutTextLine(string value)
        {
            return string.Concat("//", value.Replace("//", string.Empty));
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public static string[] CommentOutTextLineSuper(string[] lines, string start, string query, bool commentOut)
        {
            var index1 = FindIndex(lines, query, FindIndex(lines, start));
            var index2 = FindIndex(lines, query, index1++);
            lines[index1] = commentOut ? lines[index1].Replace("//", string.Empty) : CommentOutTextLine(lines[index1]);
            lines[index2] = commentOut ? lines[index2].Replace("//", string.Empty) : CommentOutTextLine(lines[index2]);
            return lines;
        }

        /// <summary>
        ///     Convert color HEX code to RGB
        /// </summary>
        /// <param name="hex">The HEX code representing the color to convert to RGB</param>
        /// <param name="alpha">Flag the code as having a lower alpha value than normal</param>
        /// <param name="pulse">Flag the color as a pulse, slightly lowering the alpha</param>
        private static string RgbConverter(string hex, bool alpha = false, bool pulse = false)
        {
            var color = ColorTranslator.FromHtml(hex);
            var alphaNew = alpha ? "200" : color.A.ToString();
            var pulseNew = pulse && color.G >= 50 ? color.G - 50 : color.G;
            return $"{color.R} {pulseNew} {color.B} {alphaNew}";
        }

        private static void SetItemEffectPosition(string file, Positions position = Positions.Bottom,
            string search = "HudItemEffectMeter")
        {
            // positions 1 = top, 2 = middle, 3 = bottom
            var lines = File.ReadAllLines(file);
            var start = FindIndex(lines, search);
            var value = position switch
            {
                Positions.Top => flawhud.Default.toggle_lower_stats ? "r70" : "c100",
                Positions.Middle => flawhud.Default.toggle_lower_stats ? "r60" : "c110",
                Positions.Bottom => flawhud.Default.toggle_lower_stats ? "r50" : "c120",
                _ => flawhud.Default.toggle_lower_stats ? "r80" : "c92"
            };
            lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
            File.WriteAllLines(file, lines);
        }

        private enum Positions
        {
            Top,
            Middle,
            Bottom,
            Default
        }

        #endregion
    }
}