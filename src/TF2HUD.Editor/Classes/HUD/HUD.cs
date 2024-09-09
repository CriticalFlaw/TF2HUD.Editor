using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HUDEditor.Models;
using Xceed.Wpf.Toolkit;
using static HUDEditor.MainWindow;

namespace HUDEditor.Classes
{
    public partial class HUD
    {
        private Grid Controls = new();
        private HUDBackground HudBackground;
        private bool IsRendered;
        private string[][] Layout;

        #region HUD PROPERTIES

        public string Name { get; set; }
        public HUDSettings Settings { get; set; }
        public double Opacity { get; set; }
        public bool Maximize { get; set; }
        public string Thumbnail { get; set; }
        public string Background { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CustomizationsFolder { get; set; }
        public string EnabledFolder { get; set; }
        public Download[] Download;
        public string GitHubUrl { get; set; }
        public string TF2HudsUrl { get; set; }
        public string ComfigHudsUrl { get; set; }
        public string SteamUrl { get; set; }
        public string DiscordUrl { get; set; }
        public Dictionary<string, Controls[]> ControlOptions;
        public readonly string[] LayoutOptions;
        public List<string> DirtyControls;
        public bool Unique;
        public readonly bool InstallCrosshairs;
        public string[] Screenshots { get; set; }

        #endregion HUD PROPERTIES

        /// <summary>
        /// Initializes the HUD object with values from the schema.
        /// </summary>
        /// <param name="name">HUD object name.</param>
        /// <param name="schema">HUD schema contents.</param>
        /// <param name="isUnique">Marks the HUD as having unique customizations.</param>
        public HUD(string name, HudJson schema, bool isUnique)
        {
            Name = schema.Name ?? name;
            Settings = new HUDSettings(Name);
            Opacity = schema.Opacity;
            Maximize = schema.Maximize;
            Thumbnail = schema.Thumbnail;
            Background = schema.Background;
            Description = schema.Description;
            Author = schema.Author;
            CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
            EnabledFolder = schema.EnabledFolder ?? string.Empty;
            Download = schema.Links.Download;
            GitHubUrl = schema.Links.GitHub ?? string.Empty;
            TF2HudsUrl = schema.Links.TF2Huds ?? string.Empty;
            ComfigHudsUrl = schema.Links.ComfigHuds ?? string.Empty;
            SteamUrl = schema.Links.Steam ?? string.Empty;
            DiscordUrl = schema.Links.Discord ?? string.Empty;
            ControlOptions = schema.Controls;
            LayoutOptions = schema.Layout;
            DirtyControls = new List<string>();
            Unique = isUnique;
            InstallCrosshairs = schema.InstallCrosshairs;
            Screenshots = schema.Screenshots;
        }

        /// <summary>
        /// Changes the preset on a given HUD.
        /// </summary>
        /// <param name="preset"></param>
        public void SetPreset(Preset preset)
        {
            Settings.Preset = preset;
            IsRendered = false;
            Controls = new Grid();
            MainWindow.Logger.Info($"Chaging {Name} to Preset-{Settings.Preset}");
        }

        /// <summary>
        /// Resets all user settings to their default values as defined in the schema.
        /// </summary>
        public void ResetAll()
        {
            foreach (var section in ControlOptions.Keys)
                for (var x = 0; x < ControlOptions[section].Length; x++)
                    ResetControl(ControlOptions[section][x]);
        }

        /// <summary>
        /// Resets a group of user settings to their default values as defined in the schema.
        /// </summary>
        private void ResetSection(string selection)
        {
            foreach (var section in ControlOptions[selection])
                ResetControl(section);
        }

        /// <summary>
        /// Resets a user setting to its default value as defined in the schema.
        /// </summary>
        private void ResetControl(Controls control)
        {
            try
            {
                switch (control.Control)
                {
                    case CheckBox check:
                        if (bool.TryParse(control.Value, out var value))
                            check.IsChecked = value;
                        Logger.Info($"Reset {control.Name} to {value}");
                        break;

                    case ColorPicker color:
                        color.SelectedColor = Utilities.ConvertToColor(control.Value);
                        Logger.Info($"Reset {control.Name} to {color.SelectedColor}");
                        break;

                    case ComboBox combo:
                        if (((ComboBoxItem)combo.Items[0]).Style == (Style)Application.Current.Resources["Crosshair"])
                            combo.SelectedValue = control.Value;
                        else
                            combo.SelectedIndex = int.Parse(control.Value);
                        Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;

                    case IntegerUpDown integer:
                        integer.Text = control.Value;
                        Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;

                    case TextBox text:
                        text.Text = control.Value;
                        Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Console.WriteLine(e);
                throw;
            }
        }
    }
}