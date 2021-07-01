using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HUDEditor.Models;
using Newtonsoft.Json.Linq;
using Xceed.Wpf.Toolkit;
using static HUDEditor.MainWindow;

namespace HUDEditor.Classes
{
    public partial class HUD
    {
        private Grid controls = new();
        private HUDBackground hudBackground;
        private bool isRendered;
        private string[][] layout;
        public readonly string[] LayoutOptions;
        public Dictionary<string, Controls[]> ControlOptions;
        public List<string> DirtyControls;

        public string Name, UpdateUrl, GitHubUrl, IssueUrl, HudsTfUrl, SteamUrl, DiscordUrl;
        public string Thumbnail, Background, CustomizationsFolder, EnabledFolder;
        public HUDSettings Settings;
        public double Opacity;
        public bool Maximize;

        public event EventHandler<Classes.HUDSettingsPreset> PresetChanged;

        /// <summary>
        ///     Initialize the HUD object with values from the JSON schema.
        /// </summary>
        /// <param name="name">Name of the HUD object.</param>
        /// <param name="schema">Contents of the HUD's schema file.</param>
        public HUD(string name, HudJson schema)
        {
            // Basic Schema Properties.
            Name = name;
            Settings = new HUDSettings(Name);
            Opacity = schema.Opacity;
            Maximize = schema.Maximize;
            ControlOptions = schema.Controls;
            LayoutOptions = schema.Layout;
            DirtyControls = new List<string>();
            Thumbnail = schema.Thumbnail;
            Background = schema.Background;

            // Download and Media Links.
            if (schema.Links is not null)
            {
                UpdateUrl = schema.Links.Update ?? string.Empty;
                GitHubUrl = schema.Links.GitHub ?? string.Empty;
                HudsTfUrl = schema.Links.HudsTF ?? string.Empty;
                SteamUrl = schema.Links.Steam ?? string.Empty;
                DiscordUrl = schema.Links.Discord ?? string.Empty;
            }

            // Customization Folder Paths.
            if (string.IsNullOrWhiteSpace(schema.CustomizationsFolder)) return;
            CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
            EnabledFolder = schema.EnabledFolder ?? string.Empty;
        }

        /// <summary>
        ///     Call to download the HUD if a URL has been provided.
        /// </summary>
        public void Update()
        {
            if (UpdateUrl != null) Utilities.DownloadHud(UpdateUrl);
        }

        /// <summary>
        /// Test everything except controls and settings. Complex fields require more testing.
        /// </summary>
        /// <param name="hud"></param>
        /// <returns></returns>
        public bool TestHUD(HUD hud)
        {
            void LogChange(string prop, string before = "", string after = "")
            {
                var message = !string.IsNullOrWhiteSpace(before) ? $" (\"{before}\" => \"{after}\")" : string.Empty;
                Logger.Info($"{Name}: {prop} has changed{message}, HUD has been updated.");
            }

            bool Compare(object obj1, object obj2, string[] ignoreList)
            {
                foreach (var field in obj1.GetType().GetFields())
                {
                    if (ignoreList.Contains(field.Name)) continue;
                    if (field.GetValue(obj1) == null && field.GetValue(obj2) == null) continue;

                    if (field.FieldType == typeof(string[]))
                    {
                        var arr1 = (string[]) field.GetValue(obj1);
                        var arr2 = (string[]) field.GetValue(obj2);

                        if (arr1 == null && arr2 != null)
                        {
                            LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                            return false;
                        }

                        if (arr1 != null && arr2 == null)
                        {
                            LogChange($"{field.Name}", "Argument 2 [0]", "*not present*");
                            return false;
                        }

                        if (arr1.Length != arr2.Length)
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

                        if (value1.Keys.Count != value2.Keys.Count)
                        {
                            LogChange($"{field.Name}.Keys.Count", value1.Keys.Count.ToString(), value2.Keys.Count.ToString());
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
                            if (value1[key].Length != value2[key].Length)
                            {
                                LogChange($"{field.Name}[\"{key}\"].Length", value1[key].Length.ToString(), value2[key].Length.ToString());
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
                        if (!CompareFiles((JObject)field.GetValue(obj1), (JObject)field.GetValue(obj2), $"{field.Name}.Files => ")) return false;
                    }
                    else if (field.FieldType == typeof(Option[]))
                    {
                        var arr1 = (Option[]) field.GetValue(obj1);
                        var arr2 = (Option[]) field.GetValue(obj2);

                        if (arr1 == null && arr2 != null)
                        {
                            LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                            return false;
                        }

                        if (arr1 != null && arr2 == null)
                        {
                            LogChange($"{field.Name}", $"{arr1}[{arr1.Length}]", "*not present*");
                            return false;
                        }

                        if (arr1.Length != arr2.Length)
                        {
                            LogChange($"{field.Name}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                            return false;
                        }

                        if (arr1.Select((t, i) => Compare(t, arr2[i], new string[] { })).Any(comparison => !comparison))
                            return false;
                    }
                    else if (field.GetValue(obj1)?.ToString() != field.GetValue(obj2)?.ToString())
                    {
                        LogChange(field.Name, field.GetValue(obj1)?.ToString(), field.GetValue(obj2)?.ToString());
                        return false;
                    }
                }
                return true;
            }

            bool CompareFiles(JObject obj1, JObject obj2, string path = "")
            {
                if (obj1 == null && obj2 == null) return true;

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
                        if (!CompareFiles(obj1[x.Key].ToObject<JObject>(), obj2[x.Key].ToObject<JObject>(), $"{path}/{x.Key}")) return false;
                    }
                    else if (x.Value.Type == JTokenType.Array && obj2[x.Key].Type == JTokenType.Array)
                    {
                        var arr1 = obj1[x.Key].ToArray();
                        var arr2 = obj2[x.Key].ToArray();

                        if (arr1.Length != arr2.Length)
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
                    else if (x.Value.ToString() != obj2[x.Key].ToString())
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

            if (equal) Logger.Info($"{Name}: no fields changed, HUD has not been updated.");

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
                        Logger.Info($"Reset {control.Name} to {value}");
                        break;

                    case TextBox text:
                        text.Text = control.Value;
                        Logger.Info($"Reset {control.Name} to \"{control.Value}\"");
                        break;

                    case ColorPicker color:
                        var colors = Array.ConvertAll(control.Value.Split(' '), byte.Parse);
                        color.SelectedColor = Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                        Logger.Info($"Reset {control.Name} to {color.SelectedColor}");
                        break;

                    case ComboBox combo:
                        if (((ComboBoxItem) combo.Items[0]).Style == (Style) Application.Current.Resources["Crosshair"])
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
    }
}