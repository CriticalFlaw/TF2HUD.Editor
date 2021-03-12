using System;
using System.IO;
using System.Windows;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    public class rayshud
    {
        /// <summary>
        ///     Disable crosshair options if the crosshair is toggled off.
        /// </summary>
        private void SetCrosshairControls()
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
        ///     Set the position of the damage value.
        /// </summary>
        private static bool DamagePosition(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var xpos = enable ? "c-188" : "c108";
                var xposMin = enable ? "c-138" : "c58";
                lines[Utilities.FindIndex(lines, "\"xpos\"", Utilities.FindIndex(lines, "DamageAccountValue"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{xpos}\"";
                lines[Utilities.FindIndex(lines, "\"xpos_minmode\"", Utilities.FindIndex(lines, "DamageAccountValue"))]
                    = $"\t\t\"xpos_minmode\"\t\t\t\"{xposMin}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_damage_pos, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the position of the metal counter.
        /// </summary>
        private static bool MetalPosition(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var xpos = enable ? "c-20" : "c200";
                var ypos = enable ? "c110" : "c130";
                var xposMin = enable ? "c-30" : "c130";
                var yposMin = enable ? "c73" : "c83";
                lines[Utilities.FindIndex(lines, "\"xpos\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{xpos}\"";
                lines[Utilities.FindIndex(lines, "\"ypos\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{ypos}\"";
                lines[Utilities.FindIndex(lines, "\"xpos_minmode\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"xpos_minmode\"\t\t\t\"{xposMin}\"";
                lines[Utilities.FindIndex(lines, "\"ypos_minmode\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"ypos_minmode\"\t\t\t\"{yposMin}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_metal_pos, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the player health style.
        /// </summary>
        private static bool HealthStyle(string path, int style)
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
        ///     Set the main menu style.
        /// </summary>
        private static bool MainMenuStyle(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index = enable ? 1 : 2;
                lines[1] = Utilities.CommentOutTextLine(lines[1]);
                lines[2] = Utilities.CommentOutTextLine(lines[2]);
                lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_main_menu, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the scoreboard style.
        /// </summary>
        private static bool ScoreboardStyle(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                lines[0] = enable
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_scoreboard, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the team and class select style.
        /// </summary>
        private static bool TeamSelect(string path, bool enable)
        {
            try
            {
                // CLASS SELECT
                var lines = File.ReadAllLines(path);
                lines[0] = enable
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(path, lines);

                // TEAM MENU
                path = string.Format(Resources.file_teammenu, MainWindow.HudPath, MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                lines[0] = enable
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_team_select, ex.Message);
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

        /// <summary>
        ///     Set the player model position and orientation.
        /// </summary>
        private static bool PlayerModelPos(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                lines[0] = enable
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(path, lines);

                path = string.Format(Resources.file_hudlayout, MainWindow.HudPath, MainWindow.HudSelection);
                lines = File.ReadAllLines(path);
                var value = enable ? 100 : 15;
                lines[Utilities.FindIndex(lines, "xpos", Utilities.FindIndex(lines, "DisguiseStatus"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_player_model_pos,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the chatbox position.
        /// </summary>
        private static bool ChatBoxPos(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var value = enable ? 315 : 25;
                lines[Utilities.FindIndex(lines, "ypos", Utilities.FindIndex(lines, "HudChat"))] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_chat_pos, ex.Message);
                return false;
            }
        }
    }
}