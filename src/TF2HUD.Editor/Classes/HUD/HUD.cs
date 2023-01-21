using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private bool isRendered;
        private string[][] Layout;

        /// <summary>
        ///     Initialize the HUD object with values from the JSON schema.
        /// </summary>
        /// <param name="name">Name of the HUD object.</param>
        /// <param name="schema">Contents of the HUD's schema file.</param>
        /// <param name="unique">Flags the HUD as having unique customizations.</param>
        public HUD(string name, HudJson schema, bool unique)
        {
            // Basic Schema Properties.
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
            HudsTfUrl = schema.Links.HudsTf ?? string.Empty;
            SteamUrl = schema.Links.Steam ?? string.Empty;
            DiscordUrl = schema.Links.Discord ?? string.Empty;
            ControlOptions = schema.Controls;
            LayoutOptions = schema.Layout;
            DirtyControls = new List<string>();
            Unique = unique;
            InstallCrosshairs = schema.InstallCrosshairs;
            Screenshots = schema.Screenshots;
        }

        public void SetPreset(Preset preset)
        {
            Settings.Preset = preset;
            isRendered = false;
            Controls = new Grid();
            MainWindow.Logger.Info($"Changed preset for {Name} to HUDSettingsPreset.{Settings.Preset}");
        }

        /// <summary>
        ///     Reset all user-settings to the default values defined in the HUD schema.
        /// </summary>
        public void ResetAll()
        {
            foreach (var section in ControlOptions.Keys)
                for (var x = 0; x < ControlOptions[section].Length; x++)
                    ResetControl(ControlOptions[section][x]);
        }

        /// <summary>
        ///     Reset selected group of user-settings to the default values defined in the HUD schema.
        /// </summary>
        private void ResetSection(string selection)
        {
            foreach (var section in ControlOptions[selection])
                ResetControl(section);
        }

        /// <summary>
        ///     Reset user-settings to the default values defined in the HUD schema.
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

                    case TextBox text:
                        text.Text = control.Value;
                        Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
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
        public string HudsTfUrl { get; set; }
        public string SteamUrl { get; set; }
        public string DiscordUrl { get; set; }
        public Dictionary<string, Controls[]> ControlOptions;
        public readonly string[] LayoutOptions;
        public List<string> DirtyControls;
        public string[] Screenshots { get; set; }
        public bool Unique;
        public readonly bool InstallCrosshairs;

        #endregion HUD PROPERTIES
    }
}