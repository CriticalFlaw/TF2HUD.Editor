using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HUDEditor.Models;
using log4net;
using Newtonsoft.Json.Linq;
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
        public HUD(string name, HudJson schema, bool uniq, ILog logger, IUtilities utilities, INotifier notifier, ILocalization localization, VTF vtf, IAppSettings settings)
        {
            // Basic Schema Properties.
            Name = schema.Name ?? name;
            Settings = new HUDSettings(Name, logger);
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
            DirtyControls = new List<string>();
            Unique = uniq;
            _logger = logger;
            _utilities = utilities;
            _notifier = notifier;
            _localization = localization;
            _vtf = vtf;
            _settings = settings;
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
        ///     Test everything except controls and settings. Complex fields require more testing.
        /// </summary>
        /// <param name="hud"></param>
        /// <returns></returns>
        public bool TestHUD(HUD hud)
        {
            void LogChange(string prop, string before = "", string after = "")
            {
                var message = !string.IsNullOrWhiteSpace(before) ? $" (\"{before}\" => \"{after}\")" : string.Empty;
                _logger.Info($"{Name}: {prop} has changed{message}, HUD has been updated.");
            }

            bool Compare(object obj1, object obj2, string[] ignoreList)
            {
                foreach (var field in obj1.GetType().GetFields())
                {
                    if (ignoreList.Contains(field.Name)) continue;
                    if (field.GetValue(obj1) is null && field.GetValue(obj2) is null) continue;

                    if (field.FieldType == typeof(string[]))
                    {
                        var arr1 = (string[]) field.GetValue(obj1);
                        var arr2 = (string[]) field.GetValue(obj2);

                        if (arr1 is null && arr2 is not null)
                        {
                            LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                            return false;
                        }

                        if (arr1 is not null && arr2 is null)
                        {
                            LogChange($"{field.Name}", "Argument 2 [0]", "*not present*");
                            return false;
                        }

                        if (!arr1.Length.Equals(arr2.Length))
                        {
                            LogChange($"{field.Name}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                            return false;
                        }

                        for (var i = 0; i < arr1.Length; i++)
                        {
                            if (arr1[i] == arr2[i]) continue;
                            LogChange($"{field.Name}[{i}]", arr1[i], arr2[i]);
                            return false;
                        }
                    }
                    else if (field.FieldType == typeof(Dictionary<string, Controls[]>))
                    {
                        var value1 = (Dictionary<string, Controls[]>) field.GetValue(obj1);
                        var value2 = (Dictionary<string, Controls[]>) field.GetValue(obj2);

                        if (!value1.Keys.Count.Equals(value2.Keys.Count))
                        {
                            LogChange($"{field.Name}.Keys.Count", value1.Keys.Count.ToString(),
                                value2.Keys.Count.ToString());
                            return false;
                        }

                        for (var i = 0; i < value1.Keys.Count; i++)
                        {
                            if (value1.Keys.ElementAt(i) == value2.Keys.ElementAt(i)) continue;
                            LogChange($"Controls[{i}].Header", value1.Keys.ElementAt(i), value2.Keys.ElementAt(i));
                            return false;
                        }

                        foreach (var key in value1.Keys)
                        {
                            if (!value1[key].Length.Equals(value2[key].Length))
                            {
                                LogChange($"{field.Name}[\"{key}\"].Length", value1[key].Length.ToString(),
                                    value2[key].Length.ToString());
                                return false;
                            }

                            for (var i = 0; i < value1[key].Length; i++)
                            {
                                var comparison = Compare(value1[key][i], value2[key][i], new[]
                                {
                                    "Control",
                                    "Value"
                                });
                                if (!comparison) return false;
                            }
                        }
                    }
                    else if (field.FieldType == typeof(JObject))
                    {
                        if (!CompareFiles((JObject) field.GetValue(obj1), (JObject) field.GetValue(obj2),
                            $"{field.Name}.Files => ")) return false;
                    }
                    else if (field.FieldType == typeof(Option[]))
                    {
                        var arr1 = (Option[]) field.GetValue(obj1);
                        var arr2 = (Option[]) field.GetValue(obj2);

                        if (arr1 is null && arr2 is not null)
                        {
                            LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                            return false;
                        }

                        if (arr1 is not null && arr2 is null)
                        {
                            LogChange($"{field.Name}", $"{arr1}[{arr1.Length}]", "*not present*");
                            return false;
                        }

                        if (!arr1.Length.Equals(arr2.Length))
                        {
                            LogChange($"{field.Name}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                            return false;
                        }

                        if (arr1.Select((t, i) => Compare(t, arr2[i], new string[] { })).Any(comparison => !comparison))
                            return false;
                    }
                    else if (!string.Equals(field.GetValue(obj1)?.ToString(), field.GetValue(obj2)?.ToString()))
                    {
                        LogChange(field.Name, field.GetValue(obj1)?.ToString(), field.GetValue(obj2)?.ToString());
                        return false;
                    }
                }

                return true;
            }

            bool CompareFiles(JObject obj1, JObject obj2, string path = "")
            {
                if (obj1 is null && obj2 is null) return true;

                foreach (var x in obj1)
                {
                    if (obj2.ContainsKey(x.Key)) continue;
                    LogChange($"{path}{x.Key}", x.Key, "*not present*");
                    return false;
                }

                foreach (var x in obj2)
                {
                    if (obj1.ContainsKey(x.Key)) continue;
                    LogChange($"{path}{x.Key}", "*not present*", x.Key);
                    return false;
                }

                foreach (var x in obj1)
                    if (obj1[x.Key].Type == JTokenType.Object && obj2[x.Key].Type == JTokenType.Object)
                    {
                        if (!CompareFiles(obj1[x.Key].ToObject<JObject>(), obj2[x.Key].ToObject<JObject>(),
                            $"{path}/{x.Key}")) return false;
                    }
                    else if (x.Value.Type == JTokenType.Array && obj2[x.Key].Type == JTokenType.Array)
                    {
                        var arr1 = obj1[x.Key].ToArray();
                        var arr2 = obj2[x.Key].ToArray();

                        if (!arr1.Length.Equals(arr2.Length))
                        {
                            LogChange($"{path}{x.Key}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                            return false;
                        }

                        for (var i = 0; i < arr1.Length; i++)
                        {
                            if (arr1[i].ToString() == arr2[i].ToString()) continue;
                            LogChange($"{path}{x.Key}/[{i}]", arr1[i].ToString(), arr2[i].ToString());
                            return false;
                        }
                    }
                    else if (!string.Equals(x.Value.ToString(), obj2[x.Key].ToString()))
                    {
                        LogChange(x.Key, x.Value.ToString(), obj2[x.Key].ToString());
                        return false;
                    }

                return true;
            }

            var equal = Compare(this, hud, new[]
            {
                "controls",
                "DirtyControls",
                "isRendered",
                "Settings"
            });

            if (equal) _logger.Info($"{Name}: no fields changed, HUD has not been updated.");

            return equal;
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
        public void ResetSection(string selection)
        {
            foreach (var section in ControlOptions[selection])
                ResetControl(section);
        }

        /// <summary>
        ///     Reset user-settings to the default values defined in the HUD schema.
        /// </summary>
        public void ResetControl(Controls control)
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
        public readonly string[] LayoutOptions;
        public List<string> DirtyControls;
        public List<object> Screenshots { get; set; } = new();
        public bool Unique;
        private readonly ILog _logger;
        private readonly IUtilities _utilities;
        private readonly INotifier _notifier;
        private readonly ILocalization _localization;
        private readonly VTF _vtf;
        private readonly IAppSettings _settings;

        #endregion
    }
}