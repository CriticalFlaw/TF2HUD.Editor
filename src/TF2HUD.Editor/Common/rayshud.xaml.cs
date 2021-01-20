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
    ///     Interaction logic for rayshud.xaml
    /// </summary>
    public partial class rayshud : UserControl
    {
        private static readonly string _hudPath = Properties.Resources.dir_custom;

        public rayshud()
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
                //            //TbStatus.Text = settings.hud_directory;
                //            //MainWindow.Logger.Info("Directory has been set to " + TbStatus.Text);
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
            MainWindow.Logger.Info("Cleaning-up rayshud directories...");

            // Clean the application directory
            if (File.Exists(Directory.GetCurrentDirectory() + "\\rayshud.zip"))
                File.Delete(Directory.GetCurrentDirectory() + "\\rayshud.zip");

            // Clean the tf/custom directory
            var settings = Settings.Default;
            var hudDirectory = Directory.Exists(_hudPath + "\\rayshud-master")
                ? _hudPath + "\\rayshud-master"
                : string.Empty;

            if (!string.IsNullOrEmpty(hudDirectory))
            {
                // Remove the previous backup if found.
                if (File.Exists(_hudPath + "\\rayshud-backup.zip"))
                    File.Delete(_hudPath + "\\rayshud-backup.zip");

                MainWindow.Logger.Info("Found a rayshud installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, _hudPath + "\\rayshud-backup.zip");
                Directory.Delete(hudDirectory, true);
                MessageBox.Show(Properties.Resources.info_create_backup, Properties.Resources.info_create_backup_title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            MainWindow.Logger.Info("Cleaning-up rayshud directories...Done!");
        }

        /// <summary>
        ///     Check if rayshud is installed in the tf/custom directory
        /// </summary>
        public static bool CheckHudPath()
        {
            return Directory.Exists(_hudPath + "\\rayshud");
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
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Reinstall" : "Install";
                BtnSave.IsEnabled = isInstalled;
                BtnUninstall.IsEnabled = isInstalled;
                //TbStatus.Text = $"rayshud is {(!isInstalled ? "not " : "")}installed...";
                Settings.Default.Save();
            }
            else
            {
                BtnInstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                //TbStatus.Text = "tf/custom directory is not set. Please click the 'Set Directory' button to set it up.";
            }
        }

        /// <summary>
        ///     Disables certain crosshair options if the crosshair is enabled
        /// </summary>
        private void SetCrosshairControls()
        {
            CbXHairHitmarker.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //IntXHairSize.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked ?? false;
        }

        private void CbUberStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (CbUberStyle.SelectedIndex)
            {
                case 0:
                    CpUberFullColor.IsEnabled = false;
                    CpUberFlash1.IsEnabled = true;
                    CpUberFlash2.IsEnabled = true;
                    break;

                case 1:
                    CpUberFullColor.IsEnabled = true;
                    CpUberFlash1.IsEnabled = false;
                    CpUberFlash2.IsEnabled = false;
                    break;

                default:
                    CpUberFullColor.IsEnabled = false;
                    CpUberFlash1.IsEnabled = false;
                    CpUberFlash2.IsEnabled = false;
                    break;
            }
        }

        #region CLICK_EVENTS

        /// <summary>
        ///     Installs rayshud to the user's tf/custom folder
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
                MainWindow.Logger.Info("Installing rayshud...");
                MainWindow.Logger.Info("Downloading the latest rayshud...");
                MainWindow.DownloadHud(Properties.Resources.download_rayshud);
                MainWindow.Logger.Info("Downloading the latest rayshud...Done!");
                MainWindow.Logger.Info("Extracting downloaded rayshud to " + _hudPath);
                MainWindow.Logger.Info("Extracting downloaded rayshud to " + _hudPath);
                ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\rayshud.zip", _hudPath);
                if (Directory.Exists(_hudPath + "\\rayshud"))
                    Directory.Delete(_hudPath + "\\rayshud", true);
                if (Directory.Exists(_hudPath + "\\rayshud-master"))
                    Directory.Move(_hudPath + "\\rayshud-master", _hudPath + "\\rayshud");
                MainWindow.Logger.Info("Extracting downloaded rayshud...Done!");
                SaveHudSettings();
                ApplyHudSettings();
                SetFormControls();

                //if (!CheckGameStatus()) return;
                //MainWindow.Logger.Info("Installing rayshud...");
                //var worker = new BackgroundWorker();
                //worker.DoWork += (o, ea) =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //    });
                //};
                //worker.RunWorkerCompleted += (o, ea) =>
                //{
                //    //TbStatus.Text = "Installation finished at " + DateTime.Now;
                //    MainWindow.Logger.Info("Installing rayshud...Done!");
                //    MessageBox.Show(TF2HUD.Editor.Properties.Resources.info_install_complete_desc,
                //        TF2HUD.Editor.Properties.Resources.info_install_complete, MessageBoxButton.OK, MessageBoxImage.Information);
                //};
                //worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Installing rayshud.", Properties.Resources.error_app_install, ex.Message);
            }
        }

        /// <summary>
        ///     Removes rayshud from the user's tf/custom folder
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MainWindow.CheckGameStatus()) return;
                MainWindow.Logger.Info("Uninstalling rayshud...");
                if (!CheckHudPath()) return;
                Directory.Delete(_hudPath + "\\rayshud", true);
                //TbStatus.Text = "Uninstalled rayshud at " + DateTime.Now;
                SetupDirectory();
                MainWindow.Logger.Info("Uninstalling rayshud...Done!");
                MessageBox.Show(Properties.Resources.info_uninstall_complete_desc,
                    Properties.Resources.info_uninstall_complete, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Uninstalling rayshud.", Properties.Resources.error_app_uninstall,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Saves then applies the rayshud settings
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveHudSettings();
            ApplyHudSettings();
            //var worker = new BackgroundWorker();
            //worker.DoWork += (o, ea) =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        SaveHudSettings();
            //        ApplyHudSettings();
            //    });
            //};
            //worker.RunWorkerCompleted += (o, ea) => { BusyIndicator.IsBusy = false; };
            //BusyIndicator.IsBusy = true;
            //worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Resets the rayshud settings to the default
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
                var settings = Properties.rayshud.Default;
                settings.color_health_normal = CpHealthNormal.SelectedColor?.ToString();
                settings.color_health_buff = CpHealthBuffed.SelectedColor?.ToString();
                settings.color_health_low = CpHealthLow.SelectedColor?.ToString();
                settings.color_ammo_clip = CpAmmoClip.SelectedColor?.ToString();
                settings.color_ammo_clip_low = CpAmmoClipLow.SelectedColor?.ToString();
                settings.color_ammo_reserve = CpAmmoReserve.SelectedColor?.ToString();
                settings.color_ammo_reserve_low = CpAmmoReserveLow.SelectedColor?.ToString();
                settings.color_uber_bar = CpUberBarColor.SelectedColor?.ToString();
                settings.color_uber_full = CpUberFullColor.SelectedColor?.ToString();
                settings.color_xhair_normal = CpXHairColor.SelectedColor?.ToString();
                settings.color_xhair_pulse = CpXHairPulse.SelectedColor?.ToString();
                settings.color_uber_flash1 = CpUberFlash1.SelectedColor?.ToString();
                settings.color_uber_flash2 = CpUberFlash2.SelectedColor?.ToString();
                settings.val_uber_animation = CbUberStyle.SelectedIndex;
                settings.val_health_style = CbHealthStyle.SelectedIndex;
                //settings.val_xhair_size = IntXHairSize.Value ?? 18;
                settings.val_xhair_style = CbXHairStyle.SelectedIndex;
                settings.val_xhair_effect = CbXHairEffect.SelectedIndex;
                settings.toggle_xhair_enable = CbXHairEnable.IsChecked ?? false;
                settings.toggle_xhair_pulse = CbXHairHitmarker.IsChecked ?? false;
                settings.toggle_disguise_image = CbDisguiseImage.IsChecked ?? false;
                settings.toggle_menu_images = CbMenuImages.IsChecked ?? false;
                settings.toggle_transparent_viewmodels = CbTransparentViewmodel.IsChecked ?? false;
                settings.toggle_damage_pos = CbDamagePos.IsChecked ?? false;
                settings.toggle_chat_bottom = CbChatBottom.IsChecked ?? false;
                settings.toggle_center_select = CbTeamCenter.IsChecked ?? false;
                settings.toggle_classic_menu = CbClassicHud.IsChecked ?? false;
                settings.toggle_min_scoreboard = CbScoreboard.IsChecked ?? false;
                settings.toggle_alt_player_model = CbPlayerModel.IsChecked ?? false;
                settings.toggle_metal_pos = CbMetalPos.IsChecked ?? false;
                settings.val_main_menu_bg = CbMainMenuBackground.SelectedIndex;
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
                var settings = Properties.rayshud.Default;
                var cc = new ColorConverter();
                //CpHealthNormal.SelectedColor = (Color)cc.ConvertFrom(settings.color_health_normal);
                //CpHealthBuffed.SelectedColor = (Color)cc.ConvertFrom(settings.color_health_buff);
                //CpHealthLow.SelectedColor = (Color)cc.ConvertFrom(settings.color_health_low);
                //CpAmmoClip.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_clip);
                //CpAmmoClipLow.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_clip_low);
                //CpAmmoReserve.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_reserve);
                //CpAmmoReserveLow.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_reserve_low);
                //CpUberBarColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_bar);
                //CpUberFullColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_full);
                //CpXHairColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_xhair_normal);
                //CpXHairPulse.SelectedColor = (Color)cc.ConvertFrom(settings.color_xhair_pulse);
                //CpUberFlash1.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_flash1);
                //CpUberFlash2.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_flash2);
                //CbUberStyle.SelectedIndex = settings.val_uber_animation;
                //CbHealthStyle.SelectedIndex = settings.val_health_style;
                ////IntXHairSize.Value = settings.val_xhair_size;
                //CbXHairStyle.SelectedIndex = settings.val_xhair_style;
                //CbXHairEffect.SelectedIndex = settings.val_xhair_effect;
                //CbXHairEnable.IsChecked = settings.toggle_xhair_enable;
                //CbXHairHitmarker.IsChecked = settings.toggle_xhair_pulse;
                //CbDisguiseImage.IsChecked = settings.toggle_disguise_image;
                //CbMenuImages.IsChecked = settings.toggle_menu_images;
                //CbTransparentViewmodel.IsChecked = settings.toggle_transparent_viewmodels;
                //CbDamagePos.IsChecked = settings.toggle_damage_pos;
                //CbChatBottom.IsChecked = settings.toggle_chat_bottom;
                //CbTeamCenter.IsChecked = settings.toggle_center_select;
                //CbClassicHud.IsChecked = settings.toggle_classic_menu;
                //CbScoreboard.IsChecked = settings.toggle_min_scoreboard;
                //CbPlayerModel.IsChecked = settings.toggle_alt_player_model;
                //CbMainMenuBackground.SelectedIndex = settings.val_main_menu_bg;
                //CbMetalPos.IsChecked = settings.toggle_metal_pos;
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
                CpHealthNormal.SelectedColor = (Color) cc.ConvertFrom("#EBE2CA");
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom("#30FF30");
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom("#FF9900");
                CpAmmoClip.SelectedColor = (Color) cc.ConvertFrom("#30FF30");
                CpAmmoClipLow.SelectedColor = (Color) cc.ConvertFrom("#FF2A82");
                CpAmmoReserve.SelectedColor = (Color) cc.ConvertFrom("#48FFFF");
                CpAmmoReserveLow.SelectedColor = (Color) cc.ConvertFrom("#FF801C");
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom("#EBE2CA");
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom("#FF3219");
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom("#F2F2F2");
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom("#FF0000");
                CpUberFlash1.SelectedColor = (Color) cc.ConvertFrom("#FFA500");
                CpUberFlash2.SelectedColor = (Color) cc.ConvertFrom("#FF4500");
                CbUberStyle.SelectedIndex = 0;
                CbHealthStyle.SelectedIndex = 0;
                //IntXHairSize.Value = 18;
                CbXHairStyle.SelectedIndex = 24;
                CbXHairEffect.SelectedIndex = 0;
                CbXHairEnable.IsChecked = false;
                CbXHairHitmarker.IsChecked = true;
                CbDisguiseImage.IsChecked = false;
                CbMenuImages.IsChecked = false;
                CbTransparentViewmodel.IsChecked = false;
                CbDamagePos.IsChecked = false;
                CbChatBottom.IsChecked = false;
                CbTeamCenter.IsChecked = false;
                CbClassicHud.IsChecked = false;
                CbScoreboard.IsChecked = false;
                CbPlayerModel.IsChecked = false;
                CbMetalPos.IsChecked = false;
                CbMainMenuBackground.SelectedIndex = 0;
                SetCrosshairControls();
                //TbStatus.Text = "Settings Reset at " + DateTime.Now;
                MainWindow.Logger.Info("Resetting HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Resetting HUD Settings.", Properties.Resources.error_app_reset,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Apply user settings to rayshud files
        /// </summary>
        private void ApplyHudSettings()
        {
            MainWindow.Logger.Info("Applying HUD Settings...");
            if (!MainMenuStyle()) return;
            if (!MainMenuClassImage()) return;
            if (!ScoreboardStyle()) return;
            if (!TeamSelect()) return;
            if (!HealthStyle()) return;
            if (!DisguiseImage()) return;
            if (!UberchargeStyle()) return;
            if (!ChatBoxPos()) return;
            //if (!Crosshair(CbXHairStyle.SelectedValue.ToString(), IntXHairSize.Value, CbXHairEffect.SelectedValue.ToString())) return;
            if (!CrosshairPulse()) return;
            if (!Colors()) return;
            if (!DamagePosition()) return;
            if (!MetalPosition()) return;
            if (!TransparentViewmodels()) return;
            if (!PlayerModelPos()) return;
            if (!MainMenuBackground()) return;
            //TbStatus.Text = "Settings Saved at " + DateTime.Now;
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
                lines[FindIndex(lines, "\"Health Normal\"")] =
                    $"\t\t\"Health Normal\"\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_health_normal)}\"";
                lines[FindIndex(lines, "\"Health Buff\"")] =
                    $"\t\t\"Health Buff\"\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "\"Health Hurt\"")] =
                    $"\t\t\"Health Hurt\"\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_health_low)}\"";
                lines[FindIndex(lines, "\"Heal Numbers\"")] =
                    $"\t\t\"Heal Numbers\"\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_health_healed)}\"";
                // Ammo
                lines[FindIndex(lines, "\"Ammo In Clip\"")] =
                    $"\t\t\"Ammo In Clip\"\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_ammo_clip)}\"";
                lines[FindIndex(lines, "\"Ammo In Reserve\"")] =
                    $"\t\t\"Ammo In Reserve\"\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_ammo_reserve)}\"";
                lines[FindIndex(lines, "\"Ammo In Clip Low\"")] =
                    $"\t\t\"Ammo In Clip Low\"\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_ammo_clip_low)}\"";
                lines[FindIndex(lines, "\"Ammo In Reserve Low\"")] =
                    $"\t\t\"Ammo In Reserve Low\"\t\t\"{RgbConverter(Properties.rayshud.Default.color_ammo_reserve_low)}\"";
                // Crosshair
                lines[FindIndex(lines, "\"Crosshair\"")] =
                    $"\t\t\"Crosshair\"\t\t\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_xhair_normal)}\"";
                lines[FindIndex(lines, "CrosshairDamage")] =
                    $"\t\t\"CrosshairDamage\"\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[FindIndex(lines, "\"Uber Bar Color\"")] =
                    $"\t\t\"Uber Bar Color\"\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_uber_bar)}\"";
                lines[FindIndex(lines, "\"Solid Color Uber\"")] =
                    $"\t\t\"Solid Color Uber\"\t\t\t\"{RgbConverter(Properties.rayshud.Default.color_uber_full)}\"";
                lines[FindIndex(lines, "\"Flashing Uber Color1\"")] =
                    $"\t\t\"Flashing Uber Color1\"\t\t\"{RgbConverter(Properties.rayshud.Default.color_uber_flash1)}\"";
                lines[FindIndex(lines, "\"Flashing Uber Color2\"")] =
                    $"\t\t\"Flashing Uber Color2\"\t\t\"{RgbConverter(Properties.rayshud.Default.color_uber_flash2)}\"";
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

                if (!Properties.rayshud.Default.toggle_xhair_enable) return true;
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

                if (!Properties.rayshud.Default.toggle_xhair_pulse) return true;
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
        ///     Set the position of the damage value
        /// </summary>
        public bool DamagePosition()
        {
            try
            {
                MainWindow.Logger.Info("Updating position of the damage value.");
                var file = _hudPath + Properties.Resources.file_huddamageaccount;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DamageAccountValue");
                var value = Properties.rayshud.Default.toggle_damage_pos ? "c-188" : "c108";
                lines[FindIndex(lines, "\"xpos\"", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Properties.rayshud.Default.toggle_damage_pos ? "c-138" : "c58";
                lines[FindIndex(lines, "\"xpos_minmode\"", start)] = $"\t\t\"xpos_minmode\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating position of the damage value.",
                    Properties.Resources.error_set_damage_pos,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the position of the metal counter
        /// </summary>
        public bool MetalPosition()
        {
            try
            {
                MainWindow.Logger.Info("Updating position of the metal counter.");
                var file = _hudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "CHudAccountPanel");
                var value = Properties.rayshud.Default.toggle_metal_pos ? "c-20" : "c200";
                lines[FindIndex(lines, "\"xpos\"", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Properties.rayshud.Default.toggle_metal_pos ? "c-30" : "c130";
                lines[FindIndex(lines, "\"xpos_minmode\"", start)] = $"\t\t\"xpos_minmode\"\t\t\t\"{value}\"";
                value = Properties.rayshud.Default.toggle_metal_pos ? "c110" : "c130";
                lines[FindIndex(lines, "\"ypos\"", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                value = Properties.rayshud.Default.toggle_metal_pos ? "c73" : "c83";
                lines[FindIndex(lines, "\"ypos_minmode\"", start)] = $"\t\t\"ypos_minmode\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating position of the damage value.",
                    Properties.Resources.error_set_damage_pos,
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

                if (!Properties.rayshud.Default.toggle_disguise_image) return true;
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
        ///     Set the player health style
        /// </summary>
        public bool HealthStyle()
        {
            try
            {
                MainWindow.Logger.Info("Updating Player Health Style.");
                var file = _hudPath + Properties.Resources.file_hudplayerhealth;
                var lines = File.ReadAllLines(file);
                var index = Properties.rayshud.Default.val_health_style - 1;
                lines[0] = CommentOutTextLine(lines[0]);
                lines[1] = CommentOutTextLine(lines[1]);
                if (Properties.rayshud.Default.val_health_style > 0)
                    lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Player Health Style.",
                    Properties.Resources.error_set_health_style,
                    ex.Message);
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
                var line1 = "\t\"$baseTexture\" \"console/backgrounds/background_modern\"";
                var line2 = "\t\"$baseTexture\" \"console/backgrounds/background_modern_widescreen\"";
                var chapterbackgrounds = _hudPath + Properties.Resources.file_chapterbackgrounds;
                var chapterbackgroundsTemp = chapterbackgrounds.Replace(".txt", ".file");

                // Restore the backgrounds to the default
                if (File.Exists(chapterbackgroundsTemp))
                    File.Move(chapterbackgroundsTemp, chapterbackgrounds);

                switch (Properties.rayshud.Default.val_main_menu_bg)
                {
                    case 1: // Classic
                        line1 = "\t\"$baseTexture\" \"console/backgrounds/background_classic\"";
                        line2 = "\t\"$baseTexture\" \"console/backgrounds/background_classic_widescreen\"";
                        break;

                    case 2: // Default
                        if (File.Exists(chapterbackgrounds))
                            File.Move(chapterbackgrounds, chapterbackgroundsTemp);
                        return true;
                }

                var file = _hudPath + Properties.Resources.file_background_upward + "upward.vmt";
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "baseTexture");
                lines[start] = line1;
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_background_upward + "upward_widescreen.vmt";
                lines = File.ReadAllLines(file);
                lines[start] = line2;
                File.WriteAllLines(file, lines);
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
                var file = _hudPath + (Properties.rayshud.Default.toggle_classic_menu
                    ? Properties.Resources.file_custom_mainmenu_classic
                    : Properties.Resources.file_custom_mainmenu);
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "TFCharacterImage");
                var value = Properties.rayshud.Default.toggle_menu_images ? "-80" : "9999";
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

                if (!Properties.rayshud.Default.toggle_transparent_viewmodels) return true;
                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(_hudPath + "\\rayshud\\cfg"))
                    Directory.CreateDirectory(_hudPath + "\\rayshud\\cfg");
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
        ///     Set the main menu style
        /// </summary>
        /// <remarks>Copy the correct background files</remarks>
        public bool MainMenuStyle()
        {
            try
            {
                MainWindow.Logger.Info("Updating Main Menu Style.");
                var file = _hudPath + Properties.Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var index = Properties.rayshud.Default.toggle_classic_menu ? 1 : 2;
                lines[1] = CommentOutTextLine(lines[1]);
                lines[2] = CommentOutTextLine(lines[2]);
                lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Main Menu Style.", Properties.Resources.error_set_main_menu,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the scoreboard style
        /// </summary>
        public bool ScoreboardStyle()
        {
            try
            {
                MainWindow.Logger.Info("Updating Scoreboard Style.");
                var file = _hudPath + Properties.Resources.file_scoreboard;
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_min_scoreboard
                    ? lines[0].Replace("//", string.Empty)
                    : CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Scoreboard Style.", Properties.Resources.error_set_scoreboard,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the team and class selection style
        /// </summary>
        public bool TeamSelect()
        {
            try
            {
                MainWindow.Logger.Info("Updating Team Selection.");

                // CLASS SELECT
                var file = _hudPath + Properties.Resources.file_classselection;
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_center_select
                    ? lines[0].Replace("//", string.Empty)
                    : CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);

                // TEAM MENU
                file = _hudPath + Properties.Resources.file_teammenu;
                lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_center_select
                    ? lines[0].Replace("//", string.Empty)
                    : CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Team Selection.", Properties.Resources.error_set_team_select,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the ÜberCharge style
        /// </summary>
        public bool UberchargeStyle()
        {
            try
            {
                MainWindow.Logger.Info("Updating ÜberCharge Animation.");
                var file = _hudPath + Properties.Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudMedicCharged");
                var index1 = FindIndex(lines, "HudMedicOrangePulseCharge", start);
                var index2 = FindIndex(lines, "HudMedicSolidColorCharge", start);
                var index3 = FindIndex(lines, "HudMedicRainbowCharged", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                lines[index3] = CommentOutTextLine(lines[index3]);
                var index = Properties.rayshud.Default.val_uber_animation switch
                {
                    1 => index2,
                    2 => index3,
                    _ => index1
                };
                lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating ÜberCharge Animation.",
                    Properties.Resources.error_set_uber_animation,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the player model position and orientation
        /// </summary>
        public bool PlayerModelPos()
        {
            try
            {
                MainWindow.Logger.Info("Updating Player Model Position.");
                var file = _hudPath + Properties.Resources.file_hudplayerclass;
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_alt_player_model
                    ? lines[0].Replace("//", string.Empty)
                    : CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);

                file = _hudPath + Properties.Resources.file_hudlayout;
                lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DisguiseStatus");
                lines[FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{(Properties.rayshud.Default.toggle_alt_player_model ? 100 : 15)}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Player Model Position.",
                    Properties.Resources.error_set_player_model_pos,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the position of the chatbox
        /// </summary>
        public bool ChatBoxPos()
        {
            try
            {
                MainWindow.Logger.Info("Updating Chatbox Position.");
                var file = _hudPath + Properties.Resources.file_basechat;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudChat");
                lines[FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(Properties.rayshud.Default.toggle_chat_bottom ? 315 : 25)}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Chatbox Position.", Properties.Resources.error_set_chatbox_pos,
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

        #endregion
    }
}