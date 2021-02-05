using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TF2HUD.Editor.Common;

namespace TF2HUD.Editor.HUDs
{
    /// <summary>
    ///     Interaction logic for rayshud.xaml
    /// </summary>
    public partial class rayshud
    {
        public rayshud()
        {
            InitializeComponent();
            ReloadHudSettings();
        }

        /// <summary>
        ///     Disable crosshair options if the crosshair is toggled off.
        /// </summary>
        private void SetCrosshairControls()
        {
            CbXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;
            IntXHairSize.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairEffect.IsEnabled = CbXHairEnable.IsChecked ?? false;
        }

        private static ItemColorList GetItemColorList()
        {
            var settings = Properties.rayshud.Default;
            return new ItemColorList
            {
                Normal = settings.color_normal,
                Unique = settings.color_unique,
                Strange = settings.color_strange,
                Vintage = settings.color_vintage,
                Haunted = settings.color_haunted,
                Genuine = settings.color_genuine,
                Collectors = settings.color_collectors,
                Unusual = settings.color_unusual,
                Community = settings.color_community,
                Valve = settings.color_valve,
                Civilian = settings.color_civilian,
                Freelance = settings.color_freelance,
                Mercenary = settings.color_mercenary,
                Commando = settings.color_commando,
                Assassin = settings.color_assassin,
                Elite = settings.color_elite
            };
        }

        #region CLICK_EVENTS

        /// <summary>
        ///     Disable crosshair options if the crosshair is toggled on.
        /// </summary>
        private void CbXHairEnable_OnClick(object sender, RoutedEventArgs e)
        {
            SetCrosshairControls();
        }

        private void CbXHairPulse_OnChecked(object sender, RoutedEventArgs e)
        {
            CpXHairPulse.IsEnabled = CbXHairPulse.IsChecked == true;
        }

        /// <summary>
        ///     Toggle ÜberCharge options depending on the style selected.
        /// </summary>
        private void CbUberStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CpUberFullColor.IsEnabled = false;
            CpUberFlash1.IsEnabled = false;
            CpUberFlash2.IsEnabled = false;

            CpUberFullColor.IsEnabled = CbUberStyle.SelectedIndex == 1;
            CpUberFlash1.IsEnabled = CbUberStyle.SelectedIndex == 0;
            CpUberFlash2.IsEnabled = CbUberStyle.SelectedIndex == 0;
        }

        #endregion CLICK_EVENTS

        #region SAVE_LOAD

        /// <summary>
        ///     Save user settings to file.
        /// </summary>
        public void SaveHudSettings()
        {
            try
            {
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
                settings.color_normal = CpItemNormal.SelectedColor?.ToString();
                settings.color_unique = CpItemUnique.SelectedColor?.ToString();
                settings.color_strange = CpItemStrange.SelectedColor?.ToString();
                settings.color_vintage = CpItemVintage.SelectedColor?.ToString();
                settings.color_haunted = CpItemHaunted.SelectedColor?.ToString();
                settings.color_genuine = CpItemGenuine.SelectedColor?.ToString();
                settings.color_collectors = CpItemCollectors.SelectedColor?.ToString();
                settings.color_unusual = CpItemUnusual.SelectedColor?.ToString();
                settings.color_community = CpItemCommunity.SelectedColor?.ToString();
                settings.color_valve = CpItemValve.SelectedColor?.ToString();
                settings.color_civilian = CpItemCivilian.SelectedColor?.ToString();
                settings.color_freelance = CpItemFreelance.SelectedColor?.ToString();
                settings.color_mercenary = CpItemMercenary.SelectedColor?.ToString();
                settings.color_commando = CpItemCommando.SelectedColor?.ToString();
                settings.color_assassin = CpItemAssassin.SelectedColor?.ToString();
                settings.color_elite = CpItemElite.SelectedColor?.ToString();
                settings.val_uber_animation = CbUberStyle.SelectedIndex;
                settings.val_health_style = CbHealthStyle.SelectedIndex;
                settings.val_xhair_size = IntXHairSize.Value ?? 18;
                settings.val_xhair_style = CbXHairStyle.SelectedIndex;
                settings.val_xhair_effect = CbXHairEffect.SelectedIndex;
                settings.toggle_xhair_enable = CbXHairEnable.IsChecked ?? false;
                settings.toggle_xhair_pulse = CbXHairPulse.IsChecked ?? false;
                settings.toggle_disguise_image = CbDisguiseImage.IsChecked ?? false;
                settings.toggle_menu_image = CbMenuImages.IsChecked ?? false;
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
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error,
                    string.Format(Properties.Resources.error_app_save, MainWindow.HudSelection), ex.Message);
            }
        }

        /// <summary>
        ///     Load user settings from file.
        /// </summary>
        public void ReloadHudSettings()
        {
            try
            {
                var settings = Properties.rayshud.Default;
                var cc = new ColorConverter();
                CpHealthNormal.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_normal);
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_buff);
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_low);
                CpAmmoClip.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_clip);
                CpAmmoClipLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_clip_low);
                CpAmmoReserve.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_reserve);
                CpAmmoReserveLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_reserve_low);
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_bar);
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_full);
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_normal);
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_pulse);
                CpUberFlash1.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_flash1);
                CpUberFlash2.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_flash2);
                CpItemNormal.SelectedColor = (Color) cc.ConvertFrom(settings.color_normal);
                CpItemUnique.SelectedColor = (Color) cc.ConvertFrom(settings.color_unique);
                CpItemStrange.SelectedColor = (Color) cc.ConvertFrom(settings.color_strange);
                CpItemVintage.SelectedColor = (Color) cc.ConvertFrom(settings.color_vintage);
                CpItemHaunted.SelectedColor = (Color) cc.ConvertFrom(settings.color_haunted);
                CpItemGenuine.SelectedColor = (Color) cc.ConvertFrom(settings.color_genuine);
                CpItemCollectors.SelectedColor = (Color) cc.ConvertFrom(settings.color_collectors);
                CpItemUnusual.SelectedColor = (Color) cc.ConvertFrom(settings.color_unusual);
                CpItemCommunity.SelectedColor = (Color) cc.ConvertFrom(settings.color_community);
                CpItemValve.SelectedColor = (Color) cc.ConvertFrom(settings.color_valve);
                CpItemCivilian.SelectedColor = (Color) cc.ConvertFrom(settings.color_civilian);
                CpItemFreelance.SelectedColor = (Color) cc.ConvertFrom(settings.color_freelance);
                CpItemMercenary.SelectedColor = (Color) cc.ConvertFrom(settings.color_mercenary);
                CpItemCommando.SelectedColor = (Color) cc.ConvertFrom(settings.color_commando);
                CpItemAssassin.SelectedColor = (Color) cc.ConvertFrom(settings.color_assassin);
                CpItemElite.SelectedColor = (Color) cc.ConvertFrom(settings.color_elite);
                CbUberStyle.SelectedIndex = settings.val_uber_animation;
                CbHealthStyle.SelectedIndex = settings.val_health_style;
                IntXHairSize.Value = settings.val_xhair_size;
                CbXHairStyle.SelectedIndex = settings.val_xhair_style;
                CbXHairEffect.SelectedIndex = settings.val_xhair_effect;
                CbXHairEnable.IsChecked = settings.toggle_xhair_enable;
                CbXHairPulse.IsChecked = settings.toggle_xhair_pulse;
                CbDisguiseImage.IsChecked = settings.toggle_disguise_image;
                CbMenuImages.IsChecked = settings.toggle_menu_image;
                CbTransparentViewmodel.IsChecked = settings.toggle_transparent_viewmodels;
                CbDamagePos.IsChecked = settings.toggle_damage_pos;
                CbChatBottom.IsChecked = settings.toggle_chat_bottom;
                CbTeamCenter.IsChecked = settings.toggle_center_select;
                CbClassicHud.IsChecked = settings.toggle_classic_menu;
                CbScoreboard.IsChecked = settings.toggle_min_scoreboard;
                CbPlayerModel.IsChecked = settings.toggle_alt_player_model;
                CbMainMenuBackground.SelectedIndex = settings.val_main_menu_bg;
                CbMetalPos.IsChecked = settings.toggle_metal_pos;
                SetCrosshairControls();
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error,
                    string.Format(Properties.Resources.error_app_load, MainWindow.HudSelection), ex.Message);
            }
        }

        /// <summary>
        ///     Reset user settings to their default values.
        /// </summary>
        /// <remarks>TODO: Default settings should be read from a JSON file.</remarks>
        public void ResetHudSettings()
        {
            try
            {
                var cc = new ColorConverter();
                var ic = new ItemColorList();
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
                CpItemNormal.SelectedColor = (Color) cc.ConvertFrom(ic.Normal);
                CpItemUnique.SelectedColor = (Color) cc.ConvertFrom(ic.Unique);
                CpItemStrange.SelectedColor = (Color) cc.ConvertFrom(ic.Strange);
                CpItemVintage.SelectedColor = (Color) cc.ConvertFrom(ic.Vintage);
                CpItemHaunted.SelectedColor = (Color) cc.ConvertFrom(ic.Haunted);
                CpItemGenuine.SelectedColor = (Color) cc.ConvertFrom(ic.Genuine);
                CpItemCollectors.SelectedColor = (Color) cc.ConvertFrom(ic.Collectors);
                CpItemUnusual.SelectedColor = (Color) cc.ConvertFrom(ic.Unusual);
                CpItemCommunity.SelectedColor = (Color) cc.ConvertFrom(ic.Community);
                CpItemValve.SelectedColor = (Color) cc.ConvertFrom(ic.Valve);
                CpItemCivilian.SelectedColor = (Color) cc.ConvertFrom(ic.Civilian);
                CpItemFreelance.SelectedColor = (Color) cc.ConvertFrom(ic.Freelance);
                CpItemMercenary.SelectedColor = (Color) cc.ConvertFrom(ic.Mercenary);
                CpItemCommando.SelectedColor = (Color) cc.ConvertFrom(ic.Commando);
                CpItemAssassin.SelectedColor = (Color) cc.ConvertFrom(ic.Assassin);
                CpItemElite.SelectedColor = (Color) cc.ConvertFrom(ic.Elite);
                CbUberStyle.SelectedIndex = 0;
                CbHealthStyle.SelectedIndex = 0;
                IntXHairSize.Value = 18;
                CbXHairStyle.SelectedIndex = 24;
                CbXHairEffect.SelectedIndex = 0;
                CbXHairEnable.IsChecked = false;
                CbXHairPulse.IsChecked = true;
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
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error,
                    string.Format(Properties.Resources.error_app_reset, MainWindow.HudSelection), ex.Message);
            }
        }

        /// <summary>
        ///     Apply user settings to the HUD files.
        /// </summary>
        public void ApplyHudSettings()
        {
            if (!MainMenuStyle()) return;
            if (!ScoreboardStyle()) return;
            if (!TeamSelect()) return;
            if (!HealthStyle()) return;
            if (!UberchargeStyle()) return;
            if (!ChatBoxPos()) return;
            if (!Colors()) return;
            if (!DamagePosition()) return;
            if (!MetalPosition()) return;
            if (!PlayerModelPos()) return;
            if (!MainMenuBackground()) return;
            if (!Common.MainMenuClassImage(Properties.rayshud.Default.toggle_menu_image)) return;
            if (!Common.DisguiseImage(Properties.rayshud.Default.toggle_disguise_image)) return;
            if (!Common.Crosshair(Properties.rayshud.Default.toggle_xhair_enable, CbXHairStyle.SelectedValue.ToString(),
                IntXHairSize.Value,
                CbXHairEffect.SelectedValue.ToString())) return;
            if (!Common.CrosshairPulse(Properties.rayshud.Default.toggle_xhair_pulse)) return;
            if (!Common.TransparentViewmodels(Properties.rayshud.Default.toggle_transparent_viewmodels)) return;
            Common.ItemColors(GetItemColorList());
        }

        #endregion SAVE_LOAD

        #region CONTROLLER

        /// <summary>
        ///     Update the client scheme colors.
        /// </summary>
        private static bool Colors()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_clientscheme_colors, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                // Health
                lines[Utilities.FindIndex(lines, "\"Health Normal\"")] =
                    $"\t\t\"Health Normal\"\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_health_normal)}\"";
                lines[Utilities.FindIndex(lines, "\"Health Buff\"")] =
                    $"\t\t\"Health Buff\"\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_health_buff)}\"";
                lines[Utilities.FindIndex(lines, "\"Health Hurt\"")] =
                    $"\t\t\"Health Hurt\"\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_health_low)}\"";
                lines[Utilities.FindIndex(lines, "\"Heal Numbers\"")] =
                    $"\t\t\"Heal Numbers\"\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_health_healed)}\"";
                // Ammo
                lines[Utilities.FindIndex(lines, "\"Ammo In Clip\"")] =
                    $"\t\t\"Ammo In Clip\"\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_ammo_clip)}\"";
                lines[Utilities.FindIndex(lines, "\"Ammo In Reserve\"")] =
                    $"\t\t\"Ammo In Reserve\"\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_ammo_reserve)}\"";
                lines[Utilities.FindIndex(lines, "\"Ammo In Clip Low\"")] =
                    $"\t\t\"Ammo In Clip Low\"\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_ammo_clip_low)}\"";
                lines[Utilities.FindIndex(lines, "\"Ammo In Reserve Low\"")] =
                    $"\t\t\"Ammo In Reserve Low\"\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_ammo_reserve_low)}\"";
                // Crosshair
                lines[Utilities.FindIndex(lines, "\"Crosshair\"")] =
                    $"\t\t\"Crosshair\"\t\t\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_xhair_normal)}\"";
                lines[Utilities.FindIndex(lines, "CrosshairDamage")] =
                    $"\t\t\"CrosshairDamage\"\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[Utilities.FindIndex(lines, "\"Uber Bar Color\"")] =
                    $"\t\t\"Uber Bar Color\"\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_uber_bar)}\"";
                lines[Utilities.FindIndex(lines, "\"Solid Color Uber\"")] =
                    $"\t\t\"Solid Color Uber\"\t\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_uber_full)}\"";
                lines[Utilities.FindIndex(lines, "\"Flashing Uber Color1\"")] =
                    $"\t\t\"Flashing Uber Color1\"\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_uber_flash1)}\"";
                lines[Utilities.FindIndex(lines, "\"Flashing Uber Color2\"")] =
                    $"\t\t\"Flashing Uber Color2\"\t\t\"{Utilities.RgbConverter(Properties.rayshud.Default.color_uber_flash2)}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_colors, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the position of the damage value.
        /// </summary>
        private static bool DamagePosition()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_huddamageaccount, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var xpos = Properties.rayshud.Default.toggle_damage_pos ? "c-188" : "c108";
                var xposMin = Properties.rayshud.Default.toggle_damage_pos ? "c-138" : "c58";
                lines[Utilities.FindIndex(lines, "\"xpos\"", Utilities.FindIndex(lines, "DamageAccountValue"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{xpos}\"";
                lines[Utilities.FindIndex(lines, "\"xpos_minmode\"", Utilities.FindIndex(lines, "DamageAccountValue"))]
                    = $"\t\t\"xpos_minmode\"\t\t\t\"{xposMin}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_damage_pos, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the position of the metal counter.
        /// </summary>
        private static bool MetalPosition()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_hudlayout, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var xpos = Properties.rayshud.Default.toggle_metal_pos ? "c-20" : "c200";
                var ypos = Properties.rayshud.Default.toggle_metal_pos ? "c110" : "c130";
                var xposMin = Properties.rayshud.Default.toggle_metal_pos ? "c-30" : "c130";
                var yposMin = Properties.rayshud.Default.toggle_metal_pos ? "c73" : "c83";
                lines[Utilities.FindIndex(lines, "\"xpos\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{xpos}\"";
                lines[Utilities.FindIndex(lines, "\"ypos\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{ypos}\"";
                lines[Utilities.FindIndex(lines, "\"xpos_minmode\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"xpos_minmode\"\t\t\t\"{xposMin}\"";
                lines[Utilities.FindIndex(lines, "\"ypos_minmode\"", Utilities.FindIndex(lines, "CHudAccountPanel"))] =
                    $"\t\t\"ypos_minmode\"\t\t\t\"{yposMin}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_metal_pos, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the player health style.
        /// </summary>
        private static bool HealthStyle()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_hudplayerhealth, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var index = Properties.rayshud.Default.val_health_style - 1;
                lines[0] = Utilities.CommentOutTextLine(lines[0]);
                lines[1] = Utilities.CommentOutTextLine(lines[1]);
                if (Properties.rayshud.Default.val_health_style > 0)
                    lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_health_style, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle custom main menu backgrounds.
        /// </summary>
        private static bool MainMenuBackground()
        {
            try
            {
                var directoryPath = new DirectoryInfo(string.Format(Properties.Resources.path_console,
                    MainWindow.HudPath, MainWindow.HudSelection));
                var chapterBackgrounds = string.Format(Properties.Resources.file_chapterbackgrounds, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var backgroundUpward = string.Format(Properties.Resources.file_background, MainWindow.HudPath,
                    MainWindow.HudSelection, "background_upward.vmt");
                var value = Properties.rayshud.Default.val_main_menu_bg == 1 ? "classic" : "modern";

                // Revert all changes before reapplying them.
                foreach (var file in directoryPath.GetFiles())
                    File.Move(file.FullName, file.FullName.Replace("bak", "vmt"));
                if (File.Exists(chapterBackgrounds.Replace("txt", "bak")))
                    File.Move(chapterBackgrounds.Replace("txt", "bak"), chapterBackgrounds);

                switch (Properties.rayshud.Default.val_main_menu_bg)
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
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_menu_background,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the main menu style.
        /// </summary>
        private static bool MainMenuStyle()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_mainmenuoverride, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var index = Properties.rayshud.Default.toggle_classic_menu ? 1 : 2;
                lines[1] = Utilities.CommentOutTextLine(lines[1]);
                lines[2] = Utilities.CommentOutTextLine(lines[2]);
                lines[index] = lines[index].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_main_menu, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the scoreboard style.
        /// </summary>
        private static bool ScoreboardStyle()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_scoreboard, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_min_scoreboard
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_scoreboard, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the team and class select style.
        /// </summary>
        private static bool TeamSelect()
        {
            try
            {
                // CLASS SELECT
                var file = string.Format(Properties.Resources.file_classselection, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_center_select
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);

                // TEAM MENU
                file = string.Format(Properties.Resources.file_teammenu, MainWindow.HudPath, MainWindow.HudSelection);
                lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_center_select
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_team_select, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the ÜberCharge style.
        /// </summary>
        private static bool UberchargeStyle()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_hudanimations, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var index1 = Utilities.FindIndex(lines, "HudMedicOrangePulseCharge",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                var index2 = Utilities.FindIndex(lines, "HudMedicSolidColorCharge",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                var index3 = Utilities.FindIndex(lines, "HudMedicRainbowCharged",
                    Utilities.FindIndex(lines, "HudMedicCharged"));
                lines[index1] = Utilities.CommentOutTextLine(lines[index1]);
                lines[index2] = Utilities.CommentOutTextLine(lines[index2]);
                lines[index3] = Utilities.CommentOutTextLine(lines[index3]);
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
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_uber_animation, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the player model position and orientation.
        /// </summary>
        private static bool PlayerModelPos()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_hudplayerclass, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                lines[0] = Properties.rayshud.Default.toggle_alt_player_model
                    ? lines[0].Replace("//", string.Empty)
                    : Utilities.CommentOutTextLine(lines[0]);
                File.WriteAllLines(file, lines);

                file = string.Format(Properties.Resources.file_hudlayout, MainWindow.HudPath, MainWindow.HudSelection);
                lines = File.ReadAllLines(file);
                var value = Properties.rayshud.Default.toggle_alt_player_model ? 100 : 15;
                lines[Utilities.FindIndex(lines, "xpos", Utilities.FindIndex(lines, "DisguiseStatus"))] =
                    $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_player_model_pos,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the chatbox position.
        /// </summary>
        private static bool ChatBoxPos()
        {
            try
            {
                var file = string.Format(Properties.Resources.file_basechat, MainWindow.HudPath,
                    MainWindow.HudSelection);
                var lines = File.ReadAllLines(file);
                var value = Properties.rayshud.Default.toggle_chat_bottom ? 315 : 25;
                lines[Utilities.FindIndex(lines, "ypos", Utilities.FindIndex(lines, "HudChat"))] =
                    $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.error_chat_pos, ex.Message);
                return false;
            }
        }

        #endregion CONTROLLER
    }
}