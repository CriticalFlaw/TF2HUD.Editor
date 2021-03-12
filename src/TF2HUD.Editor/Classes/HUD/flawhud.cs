using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    public class flawhud
    {
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
                        Classes.stock.CustomBackground();
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
                return Classes.stock.SeasonalBackgrounds(stock);
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_background,
                    ex.Message);
                return false;
            }
        }

        #region REMOVE

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

        /// <summary>
        ///     Used to determine where to position the item effect meters.
        /// </summary>
        private static void SetItemEffectPosition(string file, bool enable, Positions position = Positions.Bottom,
            string search = "HudItemEffectMeter")
        {
            // positions 1 = top, 2 = middle, 3 = bottom
            var lines = File.ReadAllLines(file);
            var start = Utilities.FindIndex(lines, search);
            var value = position switch
            {
                Positions.Top => enable ? "r70" : "c100",
                Positions.Middle => enable ? "r60" : "c110",
                Positions.Bottom => enable ? "r50" : "c120",
                _ => enable ? "r80" : "c92"
            };
            lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
            File.WriteAllLines(file, lines);
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
        ///     Toggle to Code Pro fonts instead of the default.
        /// </summary>
        private static bool CodeProFonts(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var value = enable ? "_pro" : string.Empty;
                lines[Utilities.FindIndex(lines, "clientscheme_fonts")] =
                    $"#base \"scheme/clientscheme_fonts{value}.res\"	// Change to fonts_pro.res for Code Pro fonts";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_fonts, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Lowers the player health and ammo counters.
        /// </summary>
        private static bool LowerPlayerStats(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var start = Utilities.FindIndex(lines, "HudWeaponAmmo");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r83" : "c93")}\"";
                start = Utilities.FindIndex(lines, "HudMannVsMachineStatus");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "-55" : "0")}\"";
                start = Utilities.FindIndex(lines, "CHealthAccountPanel");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r150" : "267")}\"";
                start = Utilities.FindIndex(lines, "CSecondaryTargetID");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "325" : "355")}\"";
                start = Utilities.FindIndex(lines, "HudMenuSpyDisguise");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "c60" : "c130")}\"";
                start = Utilities.FindIndex(lines, "HudSpellMenu");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\"{(enable ? "c-270" : "c-210")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_huddamageaccount, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "\"DamageAccountValue\"");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r105" : "0")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_playerhealth, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudPlayerHealth");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\"{(enable ? "r108" : "c68")}\"";
                File.WriteAllLines(path, lines);

                SetItemEffectPosition(string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                    MainWindow.HudSelection,
                    string.Empty), enable);
                SetItemEffectPosition(
                    string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                        MainWindow.HudSelection, "_cleaver"), enable,
                    Positions.Middle);
                SetItemEffectPosition(
                    string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                        MainWindow.HudSelection, "_sodapopper"), enable,
                    Positions.Top);
                SetItemEffectPosition(
                    string.Format(Resources.path_resource_ui, MainWindow.HudPath, MainWindow.HudSelection) +
                    "\\huddemomancharge.res", enable,
                    Positions.Middle,
                    "ChargeMeter");
                SetItemEffectPosition(
                    string.Format(Resources.path_resource_ui, MainWindow.HudPath, MainWindow.HudSelection) +
                    "\\huddemomanpipes.res", enable,
                    Positions.Default,
                    "PipesPresentPanel");
                SetItemEffectPosition(
                    string.Format(Resources.path_resource_ui, MainWindow.HudPath, MainWindow.HudSelection) +
                    "\\hudrocketpack.res", enable,
                    Positions.Middle);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_stats_position, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Repositions the player health and ammo counters.
        /// </summary>
        private static bool AlternatePlayerStats(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var start = Utilities.FindIndex(lines, "HudWeaponAmmo");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\"{(enable ? "r110" : "c90")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r50" : "c93")}\"";
                start = Utilities.FindIndex(lines, "HudMedicCharge");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "c60" : "c38")}\"";
                start = Utilities.FindIndex(lines, "CHealthAccountPanel");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{(enable ? "113" : "c-180")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r90" : "267")}\"";
                start = Utilities.FindIndex(lines, "DisguiseStatus");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{(enable ? "115" : "100")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r62" : "r38")}\"";
                start = Utilities.FindIndex(lines, "CMainTargetID");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r200" : "265")}\"";
                start = Utilities.FindIndex(lines, "HudAchievementTracker");
                lines[Utilities.FindIndex(lines, "NormalY", start)] =
                    $"\t\t\"NormalY\"\t\t\t\"{(enable ? "135" : "335")}\"";
                lines[Utilities.FindIndex(lines, "EngineerY", start)] =
                    $"\t\t\"EngineerY\"\t\t\t\"{(enable ? "9999" : "335")}\"";
                lines[Utilities.FindIndex(lines, "tall", start)] =
                    $"\t\t\"tall\"\t\t\t\t\"{(enable ? "9999" : "f0")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_huddamageaccount, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "\"DamageAccountValue\"");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{(enable ? "137" : "c-120")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{(enable ? "r47" : "c70")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_playerhealth, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudPlayerHealth");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\"{(enable ? "10" : "c-190")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\"{(enable ? "r75" : "c68")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_playerclass, MainWindow.HudPath,
                    MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "PlayerStatusClassImage");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\"{(enable ? "r125" : "r75")}\"";
                start = Utilities.FindIndex(lines, "classmodelpanel");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\"{(enable ? "r230" : "r200")}\"";
                lines[Utilities.FindIndex(lines, "tall", start)] =
                    $"\t\t\"tall\"\t\t\t\"{(enable ? "180" : "200")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                    MainWindow.HudSelection, string.Empty);
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\"{(enable ? "r110" : "c-60")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r65" : "c120")}\"";
                start = Utilities.FindIndex(lines, "ItemEffectMeterLabel");
                lines[Utilities.FindIndex(lines, "wide", start)] =
                    $"\t\t\"wide\"\t\t\t\t\"{(enable ? "100" : "120")}\"";
                start = Utilities.FindIndex(lines, "\"ItemEffectMeter\"");
                lines[Utilities.FindIndex(lines, "wide", start)] =
                    $"\t\t\"wide\"\t\t\t\t\"{(enable ? "100" : "120")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                    MainWindow.HudSelection, "_cleaver");
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r85" : "c110")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                    MainWindow.HudSelection, "_sodapopper");
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r75" : "c100")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_itemeffectmeter, MainWindow.HudPath,
                    MainWindow.HudSelection, "_killstreak");
                lines = File.ReadAllLines(path);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                lines[Utilities.FindIndex(lines, "xpos", start)] =
                    $"\t\t\"xpos\"\t\t\t\t\"{(enable ? "115" : "2")}\"";
                lines[Utilities.FindIndex(lines, "ypos", start)] =
                    $"\t\t\"ypos\"\t\t\t\t\"{(enable ? "r33" : "r28")}\"";
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_hudanimations, MainWindow.HudPath,
                    MainWindow.HudSelection);
                File.WriteAllText(path,
                    enable
                        ? File.ReadAllText(path).Replace("Blank", "HudBlack")
                        : File.ReadAllText(path).Replace("HudBlack", "Blank"));
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_stats_position, ex.Message);
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

        #endregion REMOVE
    }
}