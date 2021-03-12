using System;
using System.IO;
using System.Windows;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    public static class stock
    {
        /// <summary>
        ///     Set the crosshair style, position and effect.
        /// </summary>
        public static bool Crosshair(string path, bool enable, string style, int? size, string effect)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var start = Utilities.FindIndex(lines, "CustomCrosshair");
                effect = effect != "None" ? $"{effect}:ON" : "Outline:OFF";
                lines[Utilities.FindIndex(lines, "visible", start)] =
                    $"\t\t\"visible\"\t\t\t\"{Convert.ToInt32(enable)}\"";
                lines[Utilities.FindIndex(lines, "enabled", start)] =
                    $"\t\t\"enabled\"\t\t\t\"{Convert.ToInt32(enable)}\"";
                lines[Utilities.FindIndex(lines, "\"labelText\"", start)] = $"\t\t\"labelText\"\t\t\t\"{style}\"";
                lines[Utilities.FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"Size:{size} | {effect}\"";
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
        public static bool CrosshairPulse(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index1 = Utilities.FindIndex(lines, "StopEvent", Utilities.FindIndex(lines, "DamagedPlayer"));
                var index2 = Utilities.FindIndex(lines, "RunEvent", Utilities.FindIndex(lines, "DamagedPlayer"));
                lines[index1] = enable
                    ? lines[index1].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = enable
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

        /// <summary>
        ///     Toggle Spy's disguise image.
        /// </summary>
        public static bool DisguiseImage(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index1 = Utilities.FindIndex(lines, "RunEvent", Utilities.FindIndex(lines, "HudSpyDisguiseFadeIn"));
                var index2 = Utilities.FindIndex(lines, "Animate", Utilities.FindIndex(lines, "HudSpyDisguiseFadeIn"));
                var index3 = Utilities.FindIndex(lines, "RunEvent",
                    Utilities.FindIndex(lines, "HudSpyDisguiseFadeOut"));
                var index4 = Utilities.FindIndex(lines, "Animate", Utilities.FindIndex(lines, "HudSpyDisguiseFadeOut"));
                lines[index1] = enable
                    ? lines[index1].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = enable
                    ? lines[index2].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index2]);
                lines[index3] = enable
                    ? lines[index3].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index3]);
                lines[index4] = enable
                    ? lines[index4].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[index4]);
                File.WriteAllLines(path, lines);
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
        public static bool TransparentViewmodels(string path, bool enable)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                var index1 = Utilities.FindIndex(lines, "visible",
                    Utilities.FindIndex(lines, "\"TransparentViewmodel\""));
                var index2 = Utilities.FindIndex(lines, "enabled",
                    Utilities.FindIndex(lines, "\"TransparentViewmodel\""));
                lines[index1] = $"\t\t\"visible\"\t\t\t\"{Convert.ToInt32(enable)}\"";
                lines[index2] = $"\t\t\"enabled\"\t\t\t\"{Convert.ToInt32(enable)}\"";
                File.WriteAllLines(path, lines);

                path = path.Replace("\\scripts\\hudlayout.res", string.Empty);
                if (File.Exists(string.Format(Resources.file_cfg, path)))
                    File.Delete(string.Format(Resources.file_cfg, path));

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
        ///     Set the number of rows shown on the kill-feed.
        /// </summary>
        public static bool KillFeedRows(string path, int value)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                lines[Utilities.FindIndex(lines, "MaxDeathNotices", Utilities.FindIndex(lines, "HudDeathNotice"))] =
                    $"\t\t\"MaxDeathNotices\"\t\t\"{value}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_class_image, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle main menu class images.
        /// </summary>
        public static bool MainMenuClassImage(string path, bool enable, int value)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                value = enable ? value : 9999;
                lines[Utilities.FindIndex(lines, "ypos", Utilities.FindIndex(lines, "TFCharacterImage"))] =
                    $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_menu_class_image,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Update the client scheme colors.
        /// </summary>
        public static bool ItemColors(ItemColorList itemColors)
        {
            try
            {
                var file = string.Format(Resources.file_clientscheme_colors, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                // Quality
                lines[Utilities.FindIndex(lines, "\"QualityColorNormal\"")] =
                    $"\t\t\"QualityColorNormal\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Normal)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorNormal")] =
                    $"\t\t\"DimmQualityColorNormal\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Normal, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorUnique\"")] =
                    $"\t\t\"QualityColorUnique\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Unique)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorUnique")] =
                    $"\t\t\"DimmQualityColorUnique\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Unique, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorStrange\"")] =
                    $"\t\t\"QualityColorStrange\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Strange)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorStrange")] =
                    $"\t\t\"DimmQualityColorStrange\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Strange, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorVintage\"")] =
                    $"\t\t\"QualityColorVintage\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Vintage)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorVintage")] =
                    $"\t\t\"DimmQualityColorVintage\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Vintage, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorHaunted\"")] =
                    $"\t\t\"QualityColorHaunted\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Haunted)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorHaunted")] =
                    $"\t\t\"DimmQualityColorHaunted\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Haunted, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorrarity1\"")] =
                    $"\t\t\"QualityColorrarity1\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Genuine)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorrarity1")] =
                    $"\t\t\"DimmQualityColorrarity1\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Genuine, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorCollectors\"")] =
                    $"\t\t\"QualityColorCollectors\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Collectors)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorCollectors")] =
                    $"\t\t\"DimmQualityColorCollectors\"\t\t\t\"{Utilities.RgbConverter(itemColors.Collectors, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorrarity4\"")] =
                    $"\t\t\"QualityColorrarity4\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Unusual)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorrarity4")] =
                    $"\t\t\"DimmQualityColorrarity4\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Unusual, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorCommunity\"")] =
                    $"\t\t\"QualityColorCommunity\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Community)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorCommunity")] =
                    $"\t\t\"DimmQualityColorCommunity\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Community, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"QualityColorDeveloper\"")] =
                    $"\t\t\"QualityColorDeveloper\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Valve)}\"";
                lines[Utilities.FindIndex(lines, "DimmQualityColorDeveloper")] =
                    $"\t\t\"DimmQualityColorDeveloper\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Valve, 100)}\"";
                // Rarity
                lines[Utilities.FindIndex(lines, "\"ItemRarityCommon\"")] =
                    $"\t\t\"ItemRarityCommon\"\t\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Civilian)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityCommon")] =
                    $"\t\t\"DimmItemRarityCommon\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Civilian, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"ItemRarityUncommon\"")] =
                    $"\t\t\"ItemRarityUncommon\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Freelance)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityUncommon")] =
                    $"\t\t\"DimmItemRarityUncommon\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Freelance, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"ItemRarityRare\"")] =
                    $"\t\t\"ItemRarityRare\"\t\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Mercenary)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityRare")] =
                    $"\t\t\"DimmItemRarityRare\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Mercenary, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"ItemRarityMythical\"")] =
                    $"\t\t\"ItemRarityMythical\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Commando)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityMythical")] =
                    $"\t\t\"DimmItemRarityMythical\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Commando, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"ItemRarityLegendary\"")] =
                    $"\t\t\"ItemRarityLegendary\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Assassin)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityLegendary")] =
                    $"\t\t\"DimmItemRarityLegendary\"\t\t\t\t\"{Utilities.RgbConverter(itemColors.Assassin, 100)}\"";
                lines[Utilities.FindIndex(lines, "\"ItemRarityAncient\"")] =
                    $"\t\t\"ItemRarityAncient\"\t\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Elite)}\"";
                lines[Utilities.FindIndex(lines, "DimmItemRarityAncient")] =
                    $"\t\t\"DimmItemRarityAncient\"\t\t\t\t\t\"{Utilities.RgbConverter(itemColors.Elite, 100)}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Resources.error_colors, ex.Message);
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
    }
}