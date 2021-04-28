using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TF2HUD.Editor.JSON;
using TF2HUD.Editor.Properties;
using Xceed.Wpf.Toolkit;

namespace TF2HUD.Editor.Classes
{
    public partial class HUD
    {
        private readonly Grid controls = new();
        public readonly string[] LayoutOptions;
        public string Background;
        public Dictionary<string, Controls[]> ControlOptions;
        public string CustomizationsFolder, EnabledFolder;
        public List<string> DirtyControls;
        private BackgroundManager HUDBackground;
        private bool isRendered;
        private string[][] layout;
        public bool Maximize;

        public string Name;
        public double Opacity;
        public HUDSettings Settings;
        public string UpdateUrl, GitHubUrl, IssueUrl, HudsTfUrl, SteamUrl, DiscordUrl;

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

            // Download and Media Links.
            if (schema.Links is not null)
            {
                UpdateUrl = schema.Links.Update ?? string.Empty;
                GitHubUrl = schema.Links.GitHub ?? string.Empty;
                IssueUrl = schema.Links.Issue ?? string.Empty;
                HudsTfUrl = schema.Links.HudsTF ?? string.Empty;
                SteamUrl = schema.Links.Steam ?? string.Empty;
                DiscordUrl = schema.Links.Discord ?? string.Empty;
            }

            // Customization Folder Paths.
            if (!string.IsNullOrWhiteSpace(schema.CustomizationsFolder))
            {
                CustomizationsFolder = schema.CustomizationsFolder ?? string.Empty;
                EnabledFolder = schema.EnabledFolder ?? string.Empty;
            }

            // Custom Background Image.
            if (schema.Background != null) Background = schema.Background;
        }

        /// <summary>
        ///     Call to download the HUD if a URL has been provided.
        /// </summary>
        public void Update()
        {
            if (UpdateUrl != null) MainWindow.DownloadHud(UpdateUrl);
        }

        public bool TestHUD(HUD hud)
        {
            // Test everything except controls and settings
            // Complex fields require more testing
            // Always compare breadth first

            void LogChange(string prop, string before = "", string after = "")
            {
                string message = before.Length > 0 ? $" (\"{before}\" => \"{after}\")" : string.Empty;
                MainWindow.Logger.Info($"{Name}: {prop} has changed{message}, HUD has been updated.");
            }

            (bool, string) CompareTwo(object obj1, object obj2, string[] ignoreList)
            {
                foreach (var field in obj1.GetType().GetFields())
                {
                    if (ignoreList.Contains(field.Name)) continue;
                    if (field.GetValue(obj1) == null && field.GetValue(obj2) == null) continue;

                    if (field.FieldType == typeof(string[]))
                    {
                        var arr1 = field.GetValue(obj1) as string[];
                        var arr2 = field.GetValue(obj1) as string[];

                        if (arr1 != null && arr2 != null)
                        {
                            if (arr1.Length != arr2.Length)
                            {
                                LogChange($"{field.Name}.Length");
                                return (false, "field.Name");
                            }
                            for (var i = 0; i < arr1.Length; i++)
                            {
                                if (arr1[i] != arr2[i])
                                {
                                    LogChange($"{field.Name}[{i}]", arr1[i], arr2[i]);
                                    return (false, "");
                                }
                            }
                        }
                    }
                    else if (field.FieldType == typeof(Dictionary<string, Controls[]>))
                    {
                        var value1 = field.GetValue(obj1) as Dictionary<string, Controls[]>;
                        var value2 = field.GetValue(obj2) as Dictionary<string, Controls[]>;

                        if (value1.Keys.Count != value2.Keys.Count)
                        {
                            LogChange($"{field.Name}.Keys.Count", value1.Keys.Count.ToString(), value2.Keys.Count.ToString());
                            return (false, field.Name);
                        }

                        for (var i = 0; i < value1.Keys.Count; i++)
                        {
                            if (value1.Keys.ElementAt(i) != value2.Keys.ElementAt(i))
                            {
                                LogChange($"GroupBox {value1.Keys.ElementAt(i)}", value1.Keys.ElementAt(i), value2.Keys.ElementAt(i));
                                return (false, value1.Keys.ElementAt(i));
                            }
                        }

                        foreach (var key in value1.Keys)
                        {
                            if (value1[key].Length != value2[key].Length)
                            {
                                LogChange($"{field.Name}[\"{key}\"].Length", value1[key].Length.ToString(), value2[key].Length.ToString());
                                return (false, $"{field.Name}[\"{key}\"].Length");
                            }

                            for (var i = 0; i < value1[key].Length; i++)
                            {
                                var comparison = CompareTwo(value1[key][i], value2[key][i], new string[]
                                {
                                    "Control",
                                    "Value"
                                });
                                if (!comparison.Item1)
                                {
                                    return (false, comparison.Item2);
                                }
                            }
                        }
                    }
                    else if (field.FieldType == typeof(JObject))
                    {
                        var comparison = CompareFiles(field.GetValue(obj1) as JObject, field.GetValue(obj2) as JObject);

                        if (!comparison.Item1)
                        {
                            return (false, comparison.Item2);
                        }
                    }
                    else if (field.GetValue(obj1).ToString() != field.GetValue(obj2).ToString())
                    {
                        LogChange(field.Name, field.GetValue(obj1).ToString(), field.GetValue(obj2).ToString());
                        return (false, field.Name);
                    }
                }
                return (true, "");
            }

            (bool, string) CompareFiles(JObject obj1, JObject obj2, string path = "")
            {
                if (obj1 == null && obj2 == null)
                {
                    return (true, "");
                }
                foreach (var x in obj1)
                {
                    if (!obj2.ContainsKey(x.Key))
                    {
                        LogChange($"{path}{x.Key}", x.Key, "*not present*");
                        return (false, x.Key);
                    }
                }
                foreach (var x in obj2)
                {
                    if (!obj1.ContainsKey(x.Key))
                    {
                        LogChange($"{path}{x.Key}", "*not present*", x.Key);
                        return (false, x.Key);
                    }
                }
                foreach (var x in obj1)
                {
                    if (obj1[x.Key].Type == JTokenType.Object && obj2[x.Key].Type == JTokenType.Object)
                    {
                        var comparison = CompareFiles(obj1[x.Key].ToObject<JObject>(), obj2[x.Key].ToObject<JObject>(), x.Key + "/");
                        if (!comparison.Item1)
                        {
                            return (false, comparison.Item2);
                        }
                    }
                    else if (x.Value.Type == JTokenType.Array && obj2[x.Key].Type == JTokenType.Array)
                    {
                        var arr1 = obj1[x.Key].ToArray();
                        var arr2 = obj2[x.Key].ToArray();
                        if (arr1.Length != arr2.Length)
                        {
                            LogChange($"{path}{x.Key}", arr1.Length.ToString(), arr2.Length.ToString());
                            return (false, $"{path}{x.Key}");
                        }
                        for (var i = 0; i < arr1.Length; i++)
                        {
                            if (arr1[i].ToString() != arr2[i].ToString())
                            {
                                LogChange($"{path}{x.Key}/[{i}]", arr1[i].ToString(), arr2[i].ToString());
                                return (false, $"{x.Key}.Length");
                            }
                        }
                    }
                    else if (x.Value.ToString() != obj2[x.Key].ToString())
                    {
                        LogChange(x.Key, x.Value.ToString(), obj2[x.Key].ToString());
                        return (false, x.Key);
                    }
                }
                return (true, "");
            }

            var (equal, fieldChanged) = CompareTwo(this, hud, new string[]
            {
                "controls",
                "DirtyControls",
                "isRendered",
                "Settings"
            });

            if (equal)
            {
                MainWindow.Logger.Info($"{Name}: no fields changed, HUD has not been updated.");
            }

            return equal;
        }

        /// <summary>
        ///     Reset user-settings to the default values defined in the HUD schema.
        /// </summary>
        public void Reset()
        {
            try
            {
                foreach (var section in ControlOptions.Keys)
                    for (var x = 0; x < ControlOptions[section].Length; x++)
                    {
                        var controlItem = ControlOptions[section][x];
                        switch (controlItem.Control)
                        {
                            case CheckBox check:
                                if (bool.TryParse(controlItem.Value, out var value))
                                    check.IsChecked = value;
                                break;

                            case TextBox text:
                                text.Text = controlItem.Value;
                                break;

                            case ColorPicker color:
                                var colors = Array.ConvertAll(controlItem.Value.Split(' '), byte.Parse);
                                color.SelectedColor = Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                                break;

                            case ComboBox combo:
                                if (((ComboBoxItem)combo.Items[0]).Style ==
                                    (Style)Application.Current.Resources["Crosshair"])
                                    combo.SelectedValue = controlItem.Value;
                                else
                                    combo.SelectedIndex = int.Parse(controlItem.Value);
                                break;

                            case IntegerUpDown integer:
                                integer.Text = controlItem.Value;
                                break;
                        }
                    }
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                throw;
            }
        }
    }
}