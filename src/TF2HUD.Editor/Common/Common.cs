using System;
using System.IO;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Common
{
    public class Common
    {
        /// <summary>
        ///     Set the crosshair style, position and effect.
        /// </summary>
        public bool Crosshair(string style, int? size, string effect)
        {
            try
            {
                MainWindow.Logger.Info("Updating crosshair settings.");
                var file = MainWindow.HudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "CustomCrosshair");
                lines[Utilities.FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "\"labelText\"", start)] = "\t\t\"labelText\"\t\t\t\"<\"";
                lines[Utilities.FindIndex(lines, "font", start)] = "\t\t\"font\"\t\t\t\t\"Size:18 | Outline:OFF\"";
                File.WriteAllLines(file, lines);

                if (flawhud.Default.toggle_xhair_rotate) return true; // TODO
                if (!flawhud.Default.toggle_xhair_enable) return true; // TODO
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
                MainWindow.ShowErrorMessage("Error updating crosshair settings.", Resources.error_set_xhair,
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
                var file = MainWindow.HudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "DamagedPlayer");
                var index1 = Utilities.FindIndex(lines, "StopEvent", start);
                var index2 = Utilities.FindIndex(lines, "RunEvent", start);
                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_xhair_pulse) return true; //TODO
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling crosshair hitmarker.",
                    Resources.error_set_xhair_pulse,
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
                var file = MainWindow.HudPath + Resources.file_hudanimations;
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

                if (!flawhud.Default.toggle_disguise_image) return true; //TODO
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
                    Resources.error_set_spy_disguise_image, ex.Message);
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
                var file = MainWindow.HudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = Utilities.FindIndex(lines, "visible", start);
                var index2 = Utilities.FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_transparent_viewmodels) return true; //TODO
                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(MainWindow.HudPath + "\\flawhud\\cfg"))
                    Directory.CreateDirectory(MainWindow.HudPath + "\\flawhud\\cfg");
                if (File.Exists(MainWindow.HudPath + Resources.file_cfg))
                    File.Delete(MainWindow.HudPath + Resources.file_cfg);
                File.Copy(Directory.GetCurrentDirectory() + "\\hud.cfg", MainWindow.HudPath + Resources.file_cfg);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling transparent viewmodels.",
                    Resources.error_set_transparent_viewmodels, ex.Message);
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
                var file = MainWindow.HudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "HudDeathNotice");
                var value = flawhud.Default.val_killfeed_rows;
                lines[Utilities.FindIndex(lines, "MaxDeathNotices", start)] = $"\t\t\"MaxDeathNotices\"\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting the killfeed row count.",
                    Resources.error_set_menu_class_image,
                    ex.Message);
                return false;
            }
        }
    }
}