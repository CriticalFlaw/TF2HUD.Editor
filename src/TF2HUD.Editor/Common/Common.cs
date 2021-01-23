using System;
using System.IO;
using System.Windows;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Common
{
    public static class Common
    {
        /// <summary>
        ///     Set the crosshair style, position and effect.
        /// </summary>
        /// <remarks>TODO: Refactor to remove passed parameters, the options should be pulled from the HUD settings.</remarks>
        public static bool Crosshair(string style, int? size, string effect)
        {
            try
            {
                var file = string.Format(Resources.file_hudlayout, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "CustomCrosshair");
                lines[Utilities.FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "\"labelText\"", start)] = "\t\t\"labelText\"\t\t\t\"<\"";
                lines[Utilities.FindIndex(lines, "font", start)] = "\t\t\"font\"\t\t\t\t\"Size:18 | Outline:OFF\"";
                File.WriteAllLines(file, lines);

                switch (MainWindow.HudSelection.ToLowerInvariant())
                {
                    case "flawhud":
                        if (flawhud.Default.toggle_xhair_rotate) return true;
                        if (!flawhud.Default.toggle_xhair_enable) return true;
                        break;

                    case "rayshud":
                        //if (Properties.rayshud.Default.toggle_xhair_rotate) return true;
                        if (!Properties.rayshud.Default.toggle_xhair_enable) return true;
                        break;

                    default:
                        return true;
                }

                var strEffect = effect != "None" ? $"{effect}:ON" : "Outline:OFF";
                lines[Utilities.FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[Utilities.FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                lines[Utilities.FindIndex(lines, "\"labelText\"", start)] = $"\t\t\"labelText\"\t\t\t\"{style}\"";
                lines[Utilities.FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"Size:{size} | {strEffect}\"";
                File.WriteAllLines(file, lines);
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
        public static bool CrosshairPulse()
        {
            try
            {
                var file = string.Format(Resources.file_hudanimations, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "DamagedPlayer");
                var index1 = Utilities.FindIndex(lines, "StopEvent", start);
                var index2 = Utilities.FindIndex(lines, "RunEvent", start);
                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
                File.WriteAllLines(file, lines);

                switch (MainWindow.HudSelection.ToLowerInvariant())
                {
                    case "flawhud":
                        if (!flawhud.Default.toggle_xhair_pulse) return true;
                        break;

                    case "rayshud":
                        if (!Properties.rayshud.Default.toggle_xhair_pulse) return true;
                        break;

                    default:
                        return true;
                }

                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_xhair_pulse, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle Spy's disguise image.
        /// </summary>
        public static bool DisguiseImage()
        {
            try
            {
                var file = string.Format(Resources.file_hudanimations, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "HudSpyDisguiseFadeIn");
                var index1 = Utilities.FindIndex(lines, "RunEvent", start);
                var index2 = Utilities.FindIndex(lines, "Animate", start);
                start = Utilities.FindIndex(lines, "HudSpyDisguiseFadeOut");
                var index3 = Utilities.FindIndex(lines, "RunEvent", start);
                var index4 = Utilities.FindIndex(lines, "Animate", start);
                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
                lines[index3] = Utilities.CommentOutTextLine(lines[index3]);
                lines[index4] = Utilities.CommentOutTextLine(lines[index4]);
                File.WriteAllLines(file, lines);

                switch (MainWindow.HudSelection.ToLowerInvariant())
                {
                    case "flawhud":
                        if (!flawhud.Default.toggle_disguise_image) return true;
                        break;

                    case "rayshud":
                        if (!Properties.rayshud.Default.toggle_disguise_image) return true;
                        break;

                    default:
                        return true;
                }

                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                lines[index3] = lines[index3].Replace("//", string.Empty);
                lines[index4] = lines[index4].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_disguise_image, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle transparent viewmodels.
        /// </summary>
        public static bool TransparentViewmodels()
        {
            try
            {
                var file = string.Format(Resources.file_hudlayout, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = Utilities.FindIndex(lines, "visible", start);
                var index2 = Utilities.FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (File.Exists(string.Format(Resources.file_cfg, MainWindow.HudPath, MainWindow.HudSelection)))
                    File.Delete(string.Format(Resources.file_cfg, MainWindow.HudPath, MainWindow.HudSelection));

                switch (MainWindow.HudSelection.ToLowerInvariant())
                {
                    case "flawhud":
                        if (!flawhud.Default.toggle_transparent_viewmodels) return true;
                        break;

                    case "rayshud":
                        if (!Properties.rayshud.Default.toggle_transparent_viewmodels) return true;
                        break;

                    default:
                        return true;
                }

                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(MainWindow.HudPath + $"\\{MainWindow.HudSelection}\\cfg"))
                    Directory.CreateDirectory(MainWindow.HudPath + $"\\{MainWindow.HudSelection}\\cfg");
                File.Copy(Directory.GetCurrentDirectory() + "\\Resources\\hud.cfg",
                    string.Format(Resources.file_cfg, MainWindow.HudPath, MainWindow.HudSelection));
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_transparent_vm, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the number of rows shown on the killfeed.
        /// </summary>
        public static bool KillFeedRows()
        {
            try
            {
                var file = string.Format(Resources.file_hudlayout, MainWindow.HudPath, MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "HudDeathNotice");
                var value = 5;

                switch (MainWindow.HudSelection.ToLowerInvariant())
                {
                    case "flawhud":
                        value = flawhud.Default.val_killfeed_rows;
                        break;

                    case "rayshud":
                        value = Properties.rayshud.Default.val_killfeed_rows;
                        break;
                }

                lines[Utilities.FindIndex(lines, "MaxDeathNotices", start)] = $"\t\t\"MaxDeathNotices\"\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_class_image, ex.Message);
                return false;
            }
        }
    }
}