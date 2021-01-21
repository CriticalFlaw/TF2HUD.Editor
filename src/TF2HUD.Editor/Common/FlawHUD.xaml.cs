using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Common
{
    /// <summary>
    ///     Interaction logic for FlawHUD.xaml
    /// </summary>
    public partial class FlawHUD
    {
        public FlawHUD()
        {
            InitializeComponent();
            ReloadHudSettings();
            SetCrosshairControls();
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
            IntXHairSize.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
        }

        #region CLICK_EVENTS

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
        public void SaveHudSettings()
        {
            try
            {
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
                settings.val_xhair_size = IntXHairSize.Value ?? 18;
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
                settings.val_killfeed_rows = IntKillFeedRows.Value ?? 5;
                settings.Save();
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Saving HUD Settings.", Properties.Resources.error_app_save, ex.Message);
            }
        }

        /// <summary>
        ///     Load GUI with user settings from the file
        /// </summary>
        public void ReloadHudSettings()
        {
            try
            {
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
                IntXHairSize.Value = settings.val_xhair_size;
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
                IntKillFeedRows.Value = settings.val_killfeed_rows;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Loading HUD Settings.", Properties.Resources.error_app_load, ex.Message);
            }
        }

        /// <summary>
        ///     Reset user settings to their default values
        /// </summary>
        public void ResetHudSettings()
        {
            try
            {
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
                IntXHairSize.Value = 18;
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
                IntKillFeedRows.Value = 5;
                SetCrosshairControls();
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
        public void ApplyHudSettings()
        {
            var common = new Common();
            if (!MainMenuBackground()) return;
            if (!common.DisguiseImage()) return;
            if (!MainMenuClassImage()) return;
            if (!common.Crosshair(CbXHairStyle.SelectedValue.ToString(), IntXHairSize.Value,
                CbXHairEffect.SelectedValue.ToString())) return;
            if (!common.CrosshairPulse()) return;
            if (!CrosshairRotate()) return;
            if (!Colors()) return;
            if (!common.TransparentViewmodels()) return;
            if (!CodeProFonts()) return;
            if (!HealthStyle()) return;
            if (!common.KillFeedRows()) return;
            if (!LowerPlayerStats()) return;
            AlternatePlayerStats();
        }

        #endregion SAVE_LOAD

        #region CONTROLLER

        /// <summary>
        ///     Update the client scheme colors.
        /// </summary>
        private bool Colors()
        {
            try
            {
                MainWindow.Logger.Info("Updating the color client scheme.");
                var file = MainWindow.HudPath + Properties.Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[Utilities.FindIndex(lines, "\"Overheal\"")] =
                    $"\t\t\"Overheal\"\t\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_buff)}\"";
                lines[Utilities.FindIndex(lines, "OverhealPulse")] =
                    $"\t\t\"OverhealPulse\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_buff, true)}\"";
                lines[Utilities.FindIndex(lines, "\"LowHealth\"")] =
                    $"\t\t\"LowHealth\"\t\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_low)}\"";
                lines[Utilities.FindIndex(lines, "LowHealthPulse")] =
                    $"\t\t\"LowHealthPulse\"\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_low, true)}\"";
                // Ammo
                lines[Utilities.FindIndex(lines, "\"LowAmmo\"")] =
                    $"\t\t\"LowAmmo\"\t\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_ammo_low)}\"";
                lines[Utilities.FindIndex(lines, "LowAmmoPulse")] =
                    $"\t\t\"LowAmmoPulse\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_ammo_low, true)}\"";
                // Misc
                lines[Utilities.FindIndex(lines, "\"PositiveValue\"")] =
                    $"\t\t\"PositiveValue\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_buff)}\"";
                lines[Utilities.FindIndex(lines, "NegativeValue")] =
                    $"\t\t\"NegativeValue\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_health_low, true)}\"";
                lines[Utilities.FindIndex(lines, "\"TargetHealth\"")] =
                    $"\t\t\"TargetHealth\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_target_health)}\"";
                lines[Utilities.FindIndex(lines, "TargetDamage")] =
                    $"\t\t\"TargetDamage\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_target_damage)}\"";
                // Crosshair
                lines[Utilities.FindIndex(lines, "\"Crosshair\"")] =
                    $"\t\t\"Crosshair\"\t\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_xhair_normal)}\"";
                lines[Utilities.FindIndex(lines, "CrosshairDamage")] =
                    $"\t\t\"CrosshairDamage\"\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[Utilities.FindIndex(lines, "UberCharged1")] =
                    $"\t\t\"UberCharged1\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_uber_full)}\"";
                lines[Utilities.FindIndex(lines, "UberCharged2")] =
                    $"\t\t\"UberCharged2\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_uber_full, pulse: true)}\"";
                lines[Utilities.FindIndex(lines, "UberCharging")] =
                    $"\t\t\"UberCharging\"\t\t\t\t\"{Utilities.RgbConverter(flawhud.Default.color_uber_bar)}\"";
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
        private bool ColorText(bool colorText = false)
        {
            try
            {
                MainWindow.Logger.Info("Setting player health color style.");
                var file = MainWindow.HudPath + Properties.Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
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
        ///     Toggle the rotating crosshair.
        /// </summary>
        private bool CrosshairRotate()
        {
            try
            {
                MainWindow.Logger.Info("Toggling rotating crosshairs.");

                var file = MainWindow.HudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "\"Crosshair\"");
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                start = Utilities.FindIndex(lines, "\"CrosshairPulse\"");
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!flawhud.Default.toggle_xhair_enable) return true;
                if (!flawhud.Default.toggle_xhair_rotate) return true;
                start = Utilities.FindIndex(lines, "\"Crosshair\"");
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                start = Utilities.FindIndex(lines, "\"CrosshairPulse\"");
                lines[Utilities.FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[Utilities.FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
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
        ///     Toggle the custom main menu backgrounds.
        /// </summary>
        private bool MainMenuBackground()
        {
            try
            {
                MainWindow.Logger.Info("Toggling custom main menu backgrounds.");
                var directory = new DirectoryInfo(MainWindow.HudPath + Properties.Resources.dir_console);
                var chapterbackgrounds = MainWindow.HudPath + Properties.Resources.file_chapterbackgrounds;
                var chapterbackgroundsTemp = chapterbackgrounds.Replace(".txt", ".file");
                var menu = MainWindow.HudPath + Properties.Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(menu);
                var start = Utilities.FindIndex(lines, "Background");
                var index1 = Utilities.FindIndex(lines, "image", Utilities.FindIndex(lines, "if_halloween", start));
                var index2 = Utilities.FindIndex(lines, "image", Utilities.FindIndex(lines, "if_christmas", start));

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

                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
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
        private bool MainMenuClassImage()
        {
            try
            {
                MainWindow.Logger.Info("Toggling main menu class images.");
                var file = MainWindow.HudPath + Properties.Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "TFCharacterImage");
                var value = flawhud.Default.toggle_menu_images ? "-80" : "9999";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
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
        ///     Update the health indicator to use the cross style.
        /// </summary>
        private bool HealthStyle()
        {
            try
            {
                MainWindow.Logger.Info("Setting player health style.");
                var file = MainWindow.HudPath + Properties.Resources.file_playerhealth;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "\"PlayerStatusHealthBonusImage\"");
                var index = Utilities.FindIndex(lines, "image", start);
                lines[index] = "\t\t\"image\"\t\t\t\"\"";

                if (!ColorText(flawhud.Default.val_health_style == 1)) return false;

                if (flawhud.Default.val_health_style == 2)
                {
                    lines[index] = "\t\t\"image\"\t\t\t\"../hud/health_over_bg\"";
                    File.WriteAllLines(file, lines);

                    file = MainWindow.HudPath + Properties.Resources.file_hudanimations;
                    lines = File.ReadAllLines(file);
                    Utilities.CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", false);
                    Utilities.CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", false);
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
        private bool CodeProFonts()
        {
            try
            {
                MainWindow.Logger.Info("Setting to the preferred font.");
                var file = MainWindow.HudPath + Properties.Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = flawhud.Default.toggle_code_fonts ? "clientscheme_fonts_pro" : "clientscheme_fonts";
                lines[Utilities.FindIndex(lines, "clientscheme_fonts")] =
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
        ///     Lowers the player health and ammo.
        /// </summary>
        private bool LowerPlayerStats()
        {
            try
            {
                MainWindow.Logger.Info("Updating player health and ammo positions.");
                var file = MainWindow.HudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "HudWeaponAmmo");
                var value = flawhud.Default.toggle_lower_stats ? "r83" : "c93";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "HudMannVsMachineStatus");
                value = flawhud.Default.toggle_lower_stats ? "-55" : "0";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "CHealthAccountPanel");
                value = flawhud.Default.toggle_lower_stats ? "r150" : "267";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "CSecondaryTargetID");
                value = flawhud.Default.toggle_lower_stats ? "325" : "355";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "HudMenuSpyDisguise");
                value = flawhud.Default.toggle_lower_stats ? "c60" : "c130";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "HudSpellMenu");
                value = flawhud.Default.toggle_lower_stats ? "c-270" : "c-210";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "\"DamageAccountValue\"");
                value = flawhud.Default.toggle_lower_stats ? "r105" : "0";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_lower_stats ? "r108" : "c68";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                SetItemEffectPosition(string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter,
                    ""));
                SetItemEffectPosition(
                    string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, "_cleaver"),
                    Utilities.Positions.Middle);
                SetItemEffectPosition(
                    string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, "_sodapopper"),
                    Utilities.Positions.Top);
                SetItemEffectPosition(
                    MainWindow.HudPath + Properties.Resources.dir_resource_ui + "\\huddemomancharge.res",
                    Utilities.Positions.Middle,
                    "ChargeMeter");
                SetItemEffectPosition(
                    MainWindow.HudPath + Properties.Resources.dir_resource_ui + "\\huddemomanpipes.res",
                    Utilities.Positions.Default,
                    "PipesPresentPanel");
                SetItemEffectPosition(MainWindow.HudPath + Properties.Resources.dir_resource_ui + "\\hudrocketpack.res",
                    Utilities.Positions.Middle);
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
        private bool AlternatePlayerStats()
        {
            try
            {
                // Skip if the player already has "Lowered Player Stats" enabled.
                if (flawhud.Default.toggle_lower_stats) return true;
                MainWindow.Logger.Info("Repositioning player health and ammo.");
                var file = MainWindow.HudPath + Properties.Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = Utilities.FindIndex(lines, "HudWeaponAmmo");
                var value = flawhud.Default.toggle_alt_stats ? "r110" : "c90";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r50" : "c93";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "HudMedicCharge");
                value = flawhud.Default.toggle_alt_stats ? "c60" : "c38";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "CHealthAccountPanel");
                value = flawhud.Default.toggle_alt_stats ? "113" : "c-180";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r90" : "267";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "DisguiseStatus");
                value = flawhud.Default.toggle_alt_stats ? "115" : "100";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r62" : "r38";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "CMainTargetID");
                value = flawhud.Default.toggle_alt_stats ? "r200" : "265";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "HudAchievementTracker");
                value = flawhud.Default.toggle_alt_stats ? "135" : "335";
                lines[Utilities.FindIndex(lines, "NormalY", start)] = $"\t\t\"NormalY\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "9999" : "335";
                lines[Utilities.FindIndex(lines, "EngineerY", start)] = $"\t\t\"EngineerY\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "9999" : "95";
                lines[Utilities.FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_alt_stats ? "137" : "c-120";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r47" : "c70";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "\"DamageAccountValue\"");
                value = flawhud.Default.toggle_alt_stats ? "137" : "c-120";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r47" : "c70";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudPlayerHealth");
                value = flawhud.Default.toggle_alt_stats ? "10" : "c-190";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r75" : "c68";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_playerclass;
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "PlayerStatusClassImage");
                value = flawhud.Default.toggle_alt_stats ? "r125" : "r75";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "classmodelpanel");
                value = flawhud.Default.toggle_alt_stats ? "r230" : "r200";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "180" : "200";
                lines[Utilities.FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, string.Empty);
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r110" : "c-60";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r65" : "c120";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "ItemEffectMeterLabel");
                value = flawhud.Default.toggle_alt_stats ? "100" : "120";
                lines[Utilities.FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                start = Utilities.FindIndex(lines, "\"ItemEffectMeter\"");
                value = flawhud.Default.toggle_alt_stats ? "100" : "120";
                lines[Utilities.FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, "_cleaver");
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r85" : "c110";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, "_sodapopper");
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "r75" : "c100";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(MainWindow.HudPath + Properties.Resources.file_itemeffectmeter, "_killstreak");
                lines = File.ReadAllLines(file);
                start = Utilities.FindIndex(lines, "HudItemEffectMeter");
                value = flawhud.Default.toggle_alt_stats ? "115" : "2";
                lines[Utilities.FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = flawhud.Default.toggle_alt_stats ? "r33" : "r28";
                lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = MainWindow.HudPath + Properties.Resources.file_hudanimations;
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

        private static void SetItemEffectPosition(string file,
            Utilities.Positions position = Utilities.Positions.Bottom,
            string search = "HudItemEffectMeter")
        {
            // positions 1 = top, 2 = middle, 3 = bottom
            var lines = File.ReadAllLines(file);
            var start = Utilities.FindIndex(lines, search);
            var value = position switch
            {
                Utilities.Positions.Top => flawhud.Default.toggle_lower_stats ? "r70" : "c100",
                Utilities.Positions.Middle => flawhud.Default.toggle_lower_stats ? "r60" : "c110",
                Utilities.Positions.Bottom => flawhud.Default.toggle_lower_stats ? "r50" : "c120",
                _ => flawhud.Default.toggle_lower_stats ? "r80" : "c92"
            };
            lines[Utilities.FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
            File.WriteAllLines(file, lines);
        }

        #endregion
    }
}