using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    public class custom
    {
        #region flawhud

        /// <summary>
        ///     Set the player health style.
        /// </summary>
        private static bool HealthStyle(string path, int style)
        {
            try
            {
                // Apply player health and ammo colors to the text instead of the panel, if enabled.
                if (!ColorText(path, style == 1)) return false;

                // Apply the health cross, if enabled.
                var lines = File.ReadAllLines(path);
                var value = style == 2 ? "../hud/health_over_bg" : string.Empty;
                lines[
                        Utilities.FindIndex(lines, "image",
                            Utilities.FindIndex(lines, "\"PlayerStatusHealthBonusImage\""))]
                    = $"\t\t\"image\"\t\t\t\"{value}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_hudanimations, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                Utilities.CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG",
                    style != 2);
                Utilities.CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG",
                    style != 2);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_health_style, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle custom main menu backgrounds.
        /// </summary>
        private static bool MainMenuBackground(string path, string imagePath, bool custom = false, bool stock = false)
        {
            try
            {
                var directoryPath = new DirectoryInfo(string.Format(Resources.path_console,
                    MainWindow.HudPath, MainWindow.HudSelection));
                var chapterBackgrounds = string.Format(Resources.file_chapterbackgrounds, MainWindow.HudPath,
                    MainWindow.HudSelection);

                if (custom)
                {
                    if (!string.IsNullOrWhiteSpace(imagePath))
                        CustomBackground();
                }
                else
                {
                    // Revert all changes before reapplying them.
                    foreach (var file in directoryPath.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("bak", "vtf"));
                    if (File.Exists(chapterBackgrounds.Replace("txt", "bak")))
                        File.Move(chapterBackgrounds.Replace("txt", "bak"), chapterBackgrounds);

                    if (stock)
                    {
                        foreach (var file in directoryPath.GetFiles())
                            File.Move(file.FullName, file.FullName.Replace("vtf", "bak"));
                        if (File.Exists(chapterBackgrounds))
                            File.Move(chapterBackgrounds, chapterBackgrounds.Replace("txt", "bak"));
                    }
                }

                // Update the seasonal backgrounds
                return SeasonalBackgrounds(stock);
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_background,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Disable crosshair options if the crosshair is toggled off.
        /// </summary>
        private void SetCrosshairControls()
        {
            //CbXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CbXHairRotate.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //IntXHairSize.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            //CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            //CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
        }

        /// <summary>
        ///     Disable custom background options if the default background is toggled on.
        /// </summary>
        private void SetBackgroundControls()
        {
            //CbDefaultBg.IsEnabled = !CbCustomBg.IsChecked ?? false;
            //CbCustomBg.IsEnabled = !CbDefaultBg.IsChecked ?? false;
            //BtnSelectImage.IsEnabled = CbCustomBg.IsChecked ?? false;
            //BtnClearImage.IsEnabled = CbCustomBg.IsChecked ?? false;
        }

        private void CbCustomBg_OnClick()
        {
            //if (CbCustomBg.IsChecked == true)
            //    CbCustomBg.IsChecked = MainWindow.ShowMessageBox(MessageBoxImage.Warning,
            //        Properties.Resources.info_custom_background, null, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            //SetBackgroundControls();
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            using var browser = new OpenFileDialog();
            browser.ShowDialog();
            if (string.IsNullOrWhiteSpace(browser.FileName)) return;
            //LblImagePath.Content = browser.FileName;
            //Settings.Default.image_path = browser.FileName;
            //Settings.Default.Save();
            //PreviewImage.Source = new BitmapImage(new Uri(browser.FileName));
            //CbCustomBg.IsChecked = true;
            SetBackgroundControls();
        }

        private void BtnClearImage_Click(object sender, RoutedEventArgs e)
        {
            //LblImagePath.Content = string.Empty;
            //Settings.Default.image_path = string.Empty;
            //Settings.Default.Save();
            //PreviewImage.Source = null;
        }

        /// <summary>
        ///     Toggle health and ammo colors over text instead of a panel.
        /// </summary>
        private static bool ColorText(string path, bool colorText = false)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                // Panels
                Utilities.CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", !colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", !colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoBG", !colorText);
                // Text
                Utilities.CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "PlayerStatusHealthValue", colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "PlayerStatusHealthValue", colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInClip", colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInReserve", colorText);
                Utilities.CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoNoClip", colorText);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_color_text, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the rotating crosshair.
        /// </summary>
        private static bool CrosshairRotate(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var start = Utilities.FindIndex(lines, "\"Crosshair\"");
                var value = enable ? 1 : 0;
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = $"\t\t\"visible\"\t\t\t\"{value}\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = $"\t\t\"enabled\"\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "\"CrosshairPulse\"");
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = $"\t\t\"visible\"\t\t\t\"{value}\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = $"\t\t\"enabled\"\t\t\t\"{value}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_xhair, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle crosshair hitmarker.
        /// </summary>
        public static bool RotatingCrosshairPulse(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var start = Utilities.FindIndex(lines, "DamagedPlayer");
                var index1 = Utilities.FindIndex(lines, "StopEvent", start);
                var index2 = Utilities.FindIndex(lines, "RunEvent", start);
                lines[index1] = !enable
                    ? lines[index1].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = !enable
                    ? lines[index2].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index2]);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_xhair_pulse, ex.Message);
                return false;
            }
        }

        #endregion flawhud

        #region rayshud

        /// <summary>
        ///     Disable crosshair options if the crosshair is toggled off.
        /// </summary>
        private void SetCrosshairControlsRays()
        {
            //CbXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //IntXHairSize.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked ?? false;
            //CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked ?? false;
        }

        /// <summary>
        ///     Toggle ÜberCharge options depending on the style selected.
        /// </summary>
        private void CbUberStyle_SelectionChanged()
        {
            //CpUberFullColor.IsEnabled = false;
            //CpUberFlash1.IsEnabled = false;
            //CpUberFlash2.IsEnabled = false;

            //CpUberFullColor.IsEnabled = CbUberStyle.SelectedIndex == 1;
            //CpUberFlash1.IsEnabled = CbUberStyle.SelectedIndex == 0;
            //CpUberFlash2.IsEnabled = CbUberStyle.SelectedIndex == 0;
        }

        /// <summary>
        ///     Set the player health style.
        /// </summary>
        private static bool HealthStyleRays(string path, int style)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index = style - 1;
                lines[0] = Utilities.CommentOutTextLine(lines[0]);
                lines[1] = Utilities.CommentOutTextLine(lines[1]);
                if (style > 0)
                    lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_health_style, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle custom main menu backgrounds.
        /// </summary>
        private static bool MainMenuBackground(string path, int style)
        {
            try
            {
                var directoryPath = new DirectoryInfo(string.Format(Resources.path_console,
                    MainWindow.HudPath, MainWindow.HudSelection));
                var chapterBackgrounds = string.Format(Resources.file_chapterbackgrounds, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var backgroundUpward = string.Format(Resources.file_background, MainWindow.HudPath,
                    MainWindow.HudSelection, "background_upward.vmt");
                var value = style == 1 ? "classic" : "modern";

                // Revert all changes before reapplying them.
                foreach (var file in directoryPath.GetFiles())
                    File.Move(file.FullName, file.FullName.Replace("bak", "vmt"));
                if (File.Exists(chapterBackgrounds.Replace("txt", "bak")))
                    File.Move(chapterBackgrounds.Replace("txt", "bak"), chapterBackgrounds);

                switch (style)
                {
                    case 0:
                    case 1:
                        var lines = File.ReadAllLines(backgroundUpward);
                        lines[Utilities.FindIndex(lines, "baseTexture")] =
                            $"\t\"$baseTexture\" \"console/backgrounds/background_{value}\"";
                        File.WriteAllLines(backgroundUpward, lines);
                        backgroundUpward =
                            backgroundUpward.Replace("background_upward.vmt", "background_upward_widescreen.vmt");
                        lines = File.ReadAllLines(backgroundUpward);
                        lines[Utilities.FindIndex(lines, "baseTexture")] =
                            $"\t\"$baseTexture\" \"console/backgrounds/background_{value}_widescreen\"";
                        File.WriteAllLines(backgroundUpward, lines);
                        break;

                    case 2: // Default
                        foreach (var file in directoryPath.GetFiles())
                            File.Move(file.FullName, file.FullName.Replace("vmt", "bak"));
                        if (File.Exists(chapterBackgrounds))
                            File.Move(chapterBackgrounds, chapterBackgrounds.Replace("txt", "bak"));
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_background,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the ÜberCharge style.
        /// </summary>
        private static bool UberchargeStyle(string path, int style)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index1 = Utilities.FindIndex(lines, "HudMedicOrangePulseCharge",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                var index2 = Utilities.FindIndex(lines, "HudMedicSolidColorCharge",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                var index3 = Utilities.FindIndex(lines, "HudMedicRainbowCharged",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
                lines[index3] = Utilities.CommentOutTextLine(lines[index3]);
                var index = style switch
                {
                    1 => index2,
                    2 => index3,
                    _ => index1
                };
                lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_uber_animation, ex.Message);
                return false;
            }
        }

        #endregion rayshud

        #region stock

        /// <summary>
        ///     Toggle transparent viewmodels.
        /// </summary>
        public static bool TransparentViewmodels(string path, bool enable)
        {
            try
            {
                // Copy the config file required for this feature
                if (!enable) return true;
                if (!Directory.Exists(path + "\\cfg"))
                    Directory.CreateDirectory(path + "\\cfg");
                File.Copy(Directory.GetCurrentDirectory() + "\\Resources\\hud.cfg",
                    string.Format(Resources.file_cfg, path), true);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_transparent_vm, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Generate or remove a user provided background.
        /// </summary>
        public static bool CustomBackground()
        {
            try
            {
                var converter = new VTF(MainWindow.HudPath);
                var output = string.Format(Resources.file_background, MainWindow.HudPath, MainWindow.HudSelection,
                    "background_upward.vtf");
                converter.Convert(Settings.Default.image_path, output);
                File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_seasonal_backgrounds, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle seasonal (Christmas, Halloween) backgrounds.
        /// </summary>
        public static bool SeasonalBackgrounds(bool enable)
        {
            try
            {
                var menu = string.Format(Resources.file_mainmenuoverride, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(menu);
                var start = Utilities.FindIndex(lines, "Background");
                var index1 = Utilities.FindIndex(lines, "image", Utilities.FindIndex(lines, "if_halloween", start));
                var index2 = Utilities.FindIndex(lines, "image", Utilities.FindIndex(lines, "if_christmas", start));
                lines[index1] = enable
                    ? lines[index1].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = enable
                    ? lines[index2].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index2]);
                File.WriteAllLines(menu, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_seasonal_backgrounds, ex.Message);
                return false;
            }
        }

        #endregion
    }
}