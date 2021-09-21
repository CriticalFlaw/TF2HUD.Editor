using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HUDEditor.Models;
using log4net;
using Xceed.Wpf.Toolkit;

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
        public HUD(
            string name,
            HudJson schema,
            bool unique,
            ILog logger,
            IUtilities utilities,
            INotifier notifier,
            ILocalization localization,
            VTF vtf,
            IAppSettings settings,
            IUserSettingsService userSettingsService)
        {
            _logger = logger;
            _utilities = utilities;
            _notifier = notifier;
            _localization = localization;
            _vtf = vtf;
            _settings = settings;
            _userSettingsService = userSettingsService;

            ConfigureWithSchema(name, schema);
            ConfigureScreenshots(schema);

            Settings = new HUDSettings(Name, _userSettingsService);
            DirtyControls = new List<string>();
            Unique = unique;
        }

        private void ConfigureWithSchema(string hudName, HudJson schema)
        {
            Name = schema.Name ?? hudName;
            Opacity = schema.Opacity;
            Maximize = schema.Maximize;
            Thumbnail = schema.Thumbnail;
            Background = schema.Background;
            Description = schema.Description;
            Author = schema.Author;
            CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
            EnabledFolder = schema.EnabledFolder ?? string.Empty;
            UpdateUrl = schema.Links.Update ?? string.Empty;
            GitHubUrl = schema.Links.GitHub ?? string.Empty;
            HudsTfUrl = schema.Links.HudsTF ?? string.Empty;
            SteamUrl = schema.Links.Steam ?? string.Empty;
            DiscordUrl = schema.Links.Discord ?? string.Empty;
            ControlOptions = schema.Controls;
            LayoutOptions = schema.Layout;
        }

        private void ConfigureScreenshots(HudJson schema)
        {
            if (schema.Screenshots is null) return;

            var index = 0;
            foreach (var screenshot in schema.Screenshots)
            {
                Screenshots.Add(new
                {
                    ImageSource = screenshot,
                    Column = index % 2,
                    Row = index / 2
                });
                index++;
            }
        }

        public event EventHandler<HUDSettingsPreset> PresetChanged;

        /// <summary>
        ///     Call to download the HUD if a URL has been provided.
        /// </summary>
        public void Update()
        {
            if (UpdateUrl is not null) _utilities.DownloadHud(UpdateUrl);
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
                        _logger.Info($"Reset {control.Name} to {value}");
                        break;

                    case TextBox text:
                        text.Text = control.Value;
                        _logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;

                    case ColorPicker color:
                        var colors = Array.ConvertAll(control.Value.Split(' '), byte.Parse);
                        color.SelectedColor = Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                        _logger.Info($"Reset {control.Name} to {color.SelectedColor}");
                        break;

                    case ComboBox combo:
                        if (((ComboBoxItem) combo.Items[0]).Style == (Style) Application.Current.Resources["Crosshair"])
                            combo.SelectedValue = control.Value;
                        else
                            combo.SelectedIndex = int.Parse(control.Value);
                        _logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;

                    case IntegerUpDown integer:
                        integer.Text = control.Value;
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
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
        public string UpdateUrl { get; set; }
        public string GitHubUrl { get; set; }
        public string HudsTfUrl { get; set; }
        public string SteamUrl { get; set; }
        public string DiscordUrl { get; set; }
        public Dictionary<string, Controls[]> ControlOptions;
        public string[] LayoutOptions;
        public List<string> DirtyControls;
        public List<object> Screenshots { get; set; } = new();
        public bool Unique;
        private readonly ILog _logger;
        private readonly IUtilities _utilities;
        private readonly INotifier _notifier;
        private readonly ILocalization _localization;
        private readonly VTF _vtf;
        private readonly IAppSettings _settings;
        private readonly IUserSettingsService _userSettingsService;

        #endregion
    }
}