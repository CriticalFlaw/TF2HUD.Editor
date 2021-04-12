using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TF2HUD.Editor.JSON;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace TF2HUD.Editor.Classes
{
    public class HUD
    {
        private readonly Grid Controls = new();
        private readonly string[] LayoutOptions;

        public string Background;
        public Dictionary<string, Type> ConstrolList = new();
        public Dictionary<string, Controls[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
        public string Default;
        public string EnabledFolder;
        private string[][] Layout;
        public string Name;
        public HUDSettings Settings;
        public string UpdateUrl, GitHubUrl, HudsTfUrl, SteamUrl, IssueUrl;

        public HUD(string name, HudJson options)
        {
            // Validate properties from JSON
            Name = name;
            Settings = new HUDSettings(Name);

            if (options.Links is not null)
            {
                UpdateUrl = options.Links.Update ?? string.Empty;
                GitHubUrl = options.Links.GitHub ?? string.Empty;
                HudsTfUrl = options.Links.HudsTF ?? string.Empty;
                SteamUrl = options.Links.Steam ?? string.Empty;
                IssueUrl = options.Links.Issue ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(options.CustomisationsFolder))
            {
                CustomisationsFolder = options.CustomisationsFolder ?? string.Empty;
                EnabledFolder = options.EnabledFolder ?? string.Empty;
            }

            ControlOptions = options.Controls;
            LayoutOptions = options.Layout;

            if (options.Background != null) Background = options.Background;
        }

        /// <summary>
        ///     Generate the page layout using controls defined in the HUD's JSON.
        /// </summary>
        public Grid GetControls()
        {
            if (ControlsRendered)
                return Controls;

            var container = new Grid();
            var titleRowDefinition = new RowDefinition
            {
                Height = GridLength.Auto
            };
            var contentRowDefinition = new RowDefinition();
            if (Layout != null) contentRowDefinition.Height = GridLength.Auto;
            container.RowDefinitions.Add(titleRowDefinition);
            container.RowDefinitions.Add(contentRowDefinition);

            // Title
            var pageTitle = new Label
            {
                Content = Name,
                FontSize = 35,
                Margin = new Thickness(10, 10, 0, 0)
            };
            Grid.SetRow(pageTitle, 0);
            container.Children.Add(pageTitle);

            // ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic
            dynamic sectionsContainer;

            if (LayoutOptions != null)
            {
                // Splits Layout string[] into 2D Array using \s+
                Layout = LayoutOptions.Select(t => Regex.Split(t, "\\s+")).ToArray();

                sectionsContainer = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    MaxWidth = 1270,
                    MaxHeight = 720
                };

                // Assume that all row arrays are the same length, use column information from Layout[0]
                for (var i = 0; i < Layout[0].Length; i++)
                    sectionsContainer.ColumnDefinitions.Add(new ColumnDefinition());
                for (var i = 0; i < Layout.Length; i++)
                    sectionsContainer.RowDefinitions.Add(new RowDefinition());
            }
            else
            {
                sectionsContainer = new WrapPanel
                {
                    Margin = new Thickness(10)
                };
            }


            Grid.SetRow(sectionsContainer, 1);

            var lastMargin = new Thickness(10, 2, 0, 0);
            var lastTop = lastMargin.Top;

            var groupBoxIndex = 0;
            foreach (var section in ControlOptions.Keys)
            {
                var sectionContainer = new GroupBox
                {
                    Header = section,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                Panel sectionContent = new StackPanel();
                if (Layout != null) sectionContent = new WrapPanel();
                sectionContent.Margin = new Thickness(3);

                foreach (var controlItem in ControlOptions[section])
                {
                    var id = controlItem.Name;
                    var label = controlItem.Label;

                    Settings.AddSetting(controlItem.Name, controlItem);

                    switch (controlItem.Type)
                    {
                        case "Char":
                            var charContainer = new WrapPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 0)
                            };
                            var charLabel = new Label
                            {
                                Content = label,
                                Width = 60
                            };
                            var charInput = new TextBox
                            {
                                Name = id,
                                Width = 60
                            };
                            charInput.PreviewTextInput += (_, e) => e.Handled = charInput.Text != "";
                            charInput.TextChanged += (sender, e) =>
                            {
                                var input = sender as TextBox;
                                Settings.SetSetting(input.Name, input.Text);
                            };
                            charContainer.Children.Add(charLabel);
                            charContainer.Children.Add(charInput);
                            sectionContent.Children.Add(charContainer);
                            ConstrolList.Add(charInput.Name, charInput.GetType());
                            controlItem.Control = charInput;
                            break;

                        case "Checkbox":
                            var checkBoxInput = new CheckBox
                            {
                                Name = id,
                                Content = label,
                                Margin = new Thickness(10, lastTop + 10, 0, 0),
                                IsChecked = Settings.GetSetting<bool>(controlItem.Name)
                            };
                            checkBoxInput.Checked += (sender, e) =>
                            {
                                var input = sender as CheckBox;
                                Settings.SetSetting(input.Name, input.IsChecked.ToString());
                            };
                            //lastMargin = checkBoxInput.Margin;
                            sectionContent.Children.Add(checkBoxInput);
                            ConstrolList.Add(checkBoxInput.Name, checkBoxInput.GetType());
                            controlItem.Control = checkBoxInput;
                            break;

                        case "Color":
                        case "Colour":
                        case "ColorPicker":
                        case "ColourPicker":
                            var colorContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var colorLabel = new Label
                            {
                                Content = label,
                                Width = 125,
                                FontSize = 16
                            };
                            var colorInput = new ColorPicker
                            {
                                Name = id,
                                Width = 125
                            };

                            try
                            {
                                var userColor = Settings.GetSetting<Color>(id);
                                colorInput.SelectedColor = userColor;
                            }
                            catch
                            {
                                colorInput.SelectedColor = Color.FromArgb(255, 0, 255, 0);
                            }

                            colorInput.Closed += (sender, e) =>
                            {
                                var input = sender as ColorPicker;
                                Settings.SetSetting(input.Name,
                                    Utilities.RgbaConverter(input.SelectedColor.ToString()));
                            };

                            colorContainer.Children.Add(colorLabel);
                            colorContainer.Children.Add(colorInput);
                            sectionContent.Children.Add(colorContainer);
                            ConstrolList.Add(colorInput.Name, colorInput.GetType());
                            controlItem.Control = colorInput;
                            break;

                        case "DropDown":
                        case "DropDownMenu":
                        case "Select":
                        case "ComboBox":
                        case "Crosshair":
                            var comboBoxContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var comboBoxLabel = new Label
                            {
                                Content = label,
                                Width = 150,
                                FontSize = 16
                            };
                            if (controlItem.Options == null) break;
                            if (controlItem.Options.Length <= 0) break;

                            var comboBoxInput = new ComboBox
                            {
                                Name = id,
                                Width = 150,
                            };
                            foreach (var option in controlItem.Options)
                            {
                                var item = new ComboBoxItem
                                {
                                    Content = option.Label
                                };
                                if (string.Equals(controlItem.Type, "Crosshair", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    comboBoxInput.Style = (Style) Application.Current.Resources["CrosshairBox"];
                                    item.Style = (Style) Application.Current.Resources["Crosshair"];
                                }

                                comboBoxInput.Items.Add(item);
                            }

                            var setting = Settings.GetSetting<string>(controlItem.Name);
                            var containsLetters = Regex.IsMatch(setting, "\\D");

                            if (!containsLetters)
                            {
                                comboBoxInput.SelectedIndex = int.Parse(setting);
                            }
                            else
                            {
                                comboBoxInput.SelectedValue = setting;
                            }

                            comboBoxInput.SelectionChanged += (sender, e) =>
                            {
                                var input = sender as ComboBox;
                                Settings.SetSetting(input.Name, comboBoxInput.SelectedValue.ToString());
                            };

                            comboBoxContainer.Children.Add(comboBoxLabel);
                            comboBoxContainer.Children.Add(comboBoxInput);
                            sectionContent.Children.Add(comboBoxContainer);
                            ConstrolList.Add(comboBoxInput.Name, comboBoxInput.GetType());
                            controlItem.Control = comboBoxInput;
                            break;

                        case "Number":
                        case "Integer":
                        case "IntegerUpDown":
                            var integerContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var integerLabel = new Label
                            {
                                Content = label,
                                Width = 100,
                                FontSize = 16
                            };
                            var integerInput = new IntegerUpDown
                            {
                                Name = id,
                                Width = 100,
                                Value = Settings.GetSetting<int>(controlItem.Name),
                                Minimum = controlItem.Minimum,
                                Maximum = controlItem.Maximum,
                                Increment = controlItem.Increment
                            };

                            integerInput.ValueChanged += (sender, e) =>
                            {
                                var input = sender as IntegerUpDown;
                                Settings.SetSetting(input.Name, input.Text);
                            };

                            integerContainer.Children.Add(integerLabel);
                            integerContainer.Children.Add(integerInput);
                            sectionContent.Children.Add(integerContainer);
                            ConstrolList.Add(integerInput.Name, integerInput.GetType());
                            controlItem.Control = integerInput;
                            break;

                        default:
                            throw new Exception($"Entered type {controlItem.Type} is invalid.");
                    }
                }

                sectionContainer.Content = sectionContent;

                if (Layout != null)
                {
                    // Avoid evaluating unnecessarily
                    var groupBoxItemEvaluated = false;

                    for (var i = 0; i < Layout.Length; i++)
                    for (var j = 0; j < Layout[i].Length; j++)
                    {
                        // Allow index and grid area for grid coordinates
                        if (groupBoxIndex.ToString() == Layout[i][j] ||
                            section == Layout[i][j] && !groupBoxItemEvaluated)
                        {
                            // Don't set column or row if it has already been set
                            // setting the column/row every time will break spans
                            if (Grid.GetColumn(sectionContainer) == 0) Grid.SetColumn(sectionContainer, j);
                            if (Grid.GetRow(sectionContainer) == 0) Grid.SetRow(sectionContainer, i);

                            // These are not optimal speed but the code should be easier to understand:
                            // Counts the occurrences of the current item id/index
                            var columnSpan = 0;
                            // Iterate current row
                            for (var index = 0; index < Layout[i].Length; index++)
                                if (groupBoxIndex.ToString() == Layout[i][index] || section == Layout[i][index])
                                    columnSpan++;
                            Grid.SetColumnSpan(sectionContainer, columnSpan);

                            var rowSpan = 0;
                            for (var TempRowIndex = 0; TempRowIndex < Layout.Length; TempRowIndex++)
                                if (groupBoxIndex.ToString() == Layout[TempRowIndex][j] ||
                                    section == Layout[TempRowIndex][j])
                                    rowSpan++;
                            Grid.SetRowSpan(sectionContainer, rowSpan);

                            // Break parent loop
                            groupBoxItemEvaluated = true;
                            break;
                        }

                        if (groupBoxItemEvaluated) break;
                    }
                }

                sectionsContainer.Children.Add(sectionContainer);
                groupBoxIndex++;
            }

            container.Children.Add(sectionsContainer);
            Controls.Children.Add(container);

            ControlsRendered = true;
            return Controls;
        }

        /// <summary>
        ///     Call to download the latest version of a given HUD if a URL is defined.
        /// </summary>
        public void Update()
        {
            if (UpdateUrl != null) MainWindow.DownloadHud(UpdateUrl);
        }

        /// <summary>
        ///     Reset user-settings to the default as defined in the HUD's Json file.
        /// </summary>
        public void Reset()
        {
            try
            {
                foreach (var Section in ControlOptions.Keys)
                    for (var i = 0; i < ControlOptions[Section].Length; i++)
                    {
                        var controlItem = ControlOptions[Section][i];
                        var control = controlItem.Control;
                        switch (control)
                        {
                            case CheckBox check:
                                if (bool.TryParse(controlItem.Default, out var value))
                                    check.IsChecked = value;
                                break;

                            case TextBox text:
                                text.Text = controlItem.Default;
                                break;

                            case ColorPicker color:
                                var colors = Array.ConvertAll(controlItem.Default.Split(' '), c => byte.Parse(c));
                                color.SelectedColor = Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                                break;

                            case ComboBox combo:
                                if (((ComboBoxItem) combo.Items[0]).Style ==
                                    (Style) Application.Current.Resources["Crosshair"])
                                    combo.SelectedValue = controlItem.Default;
                                else
                                    combo.SelectedIndex = int.Parse(controlItem.Default);
                                break;

                            case IntegerUpDown integer:
                                integer.Text = controlItem.Default;
                                break;
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Apply user-set customizations to the HUD files.
        /// </summary>
        public bool ApplyCustomization()
        {
            try
            {
                var path = $"{MainWindow.HudPath}\\{Name}\\";

                var userSettings = JsonConvert
                    .DeserializeObject<UserJson>(File.ReadAllText($"{System.Windows.Forms.Application.LocalUserAppDataPath}\\settings.json")).Settings
                    .Where(x => x.HUD == Name);
                var hudSettings = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                    .Controls.Values;

                // If the developer defined customization folders for their HUD, then copy those files.
                if (!string.IsNullOrWhiteSpace(CustomisationsFolder))
                    MoveCustomizationFiles(path, userSettings, hudSettings);

                // This Dictionary contains folders/files/properties as they should be written to the hud
                // the 'IterateFolder' and 'IterateHUDFileProperties' will write the properties to this
                var hudFolders = new Dictionary<string, dynamic>();

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    var user = Settings.GetSetting(control.Name);
                    if (user is not null)
                        WriteToFile(control, user, hudFolders);
                }

                void IterateProperties(Dictionary<string, dynamic> folder, string folderPath)
                {
                    foreach (var property in folder.Keys)
                        if (folder[property].GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            if (property.Contains("."))
                            {
                                var filePath = folderPath + "\\" + property;
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                                // Read file, check each topmost element until we come to an element that matches
                                // the pattern (Resource/UI/HudFile.res) which indicates it's a HUD ui file
                                // if it IS a ui file, create a Dictionary to contain the elements specified in
                                // 'folder[property]', then merge the 2 Dictionaries.
                                // if the file has no matching top level elements, VDF.Stringify directly
                                // -Revan

                                if (File.Exists(filePath))
                                {
                                    var obj = VDF.Parse(File.ReadAllText(filePath));

                                    // Initialize to null to check whether matching element has been found
                                    Dictionary<string, dynamic> hudContainer = null;
                                    var pattern = @"(Resource/UI/)*.res";

                                    int preventInfinite = 0, len = obj.Keys.Count;
                                    while (hudContainer == null && preventInfinite < len)
                                    {
                                        var key = obj.Keys.ElementAt(preventInfinite);

                                        // Match pattern here, also ensure item is a HUD element
                                        if (Regex.IsMatch(key, pattern) &&
                                            obj[key].GetType() == typeof(Dictionary<string, dynamic>))
                                        {
                                            // Initialise hudContainer and create inner Dictionary
                                            //  to contain elements specified
                                            hudContainer = new Dictionary<string, dynamic>();
                                            hudContainer[key] = folder[property];
                                        }

                                        preventInfinite++;
                                    }

                                    if (hudContainer != null)
                                    {
                                        Utilities.Merge(obj, hudContainer);
                                        File.WriteAllText(filePath, VDF.Stringify(obj));
                                    }
                                    else
                                    {
                                        // Write folder[property] to hud file
                                        Utilities.Merge(obj, folder[property]);
                                        File.WriteAllText(filePath, VDF.Stringify(obj));
                                    }
                                }
                                else
                                {
                                    File.WriteAllText(filePath, VDF.Stringify(folder[property]));
                                }
                            }
                            else
                            {
                                IterateProperties(folder[property], folderPath + "\\" + property);
                            }
                        }
                }

                // write hudFolders to the HUD once instead of each WriteToFile call reading and writing
                var hudPath = MainWindow.HudPath + "\\" + Name;
                IterateProperties(hudFolders, hudPath);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        ///     Copy files used for folder-based customizations.
        /// </summary>
        public bool MoveCustomizationFiles(string path, IEnumerable<Setting> userSettings,
            Dictionary<string, Controls[]>.ValueCollection hudSettings)
        {
            try
            {
                // Check if the customization folders are valid.
                if (!Directory.Exists($"{path}\\{CustomisationsFolder}")) return true;

                var controlFilter = new List<string>
                {
                    "CheckBox",
                    "ComboBox"
                };
                userSettings = userSettings.Where(x => x.HUD == Name).Where(z =>
                    controlFilter.Contains(z.Type, StringComparer.CurrentCultureIgnoreCase));

                foreach (var group in hudSettings)
                foreach (var control in group.Where(x =>
                    controlFilter.Contains(x.Type, StringComparer.CurrentCultureIgnoreCase)))
                {
                    var userSetting = userSettings.Where(x => x.Name == control.Name).First();
                    if (userSetting is null) continue; // File name not found, skipping.

                    var custom = $"{path}{CustomisationsFolder}";
                    var enabled = $"{path}{EnabledFolder}";

                    switch (control.Type.ToLowerInvariant())
                    {
                        case "checkbox":
                            var fileName = Utilities.GetFileNames(control);
                            if (fileName is null || fileName is not string) continue; // File name not found, skipping.

                            custom += $"\\{fileName}.res";
                            enabled += $"\\{fileName}.res";

                            if (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase)) // Move to enabled
                            {
                                if (File.Exists(custom)) File.Move(custom, enabled);
                            }
                            else // Move to customization folder
                            {
                                if (File.Exists(enabled)) File.Move(enabled, custom);
                            }

                            break;

                        case "combobox":
                            var fileNames = Utilities.GetFileNames(control);
                            if (fileNames is null || fileNames is not string[])
                                continue; // File name not found, skipping.

                            foreach (string file in fileNames)
                            {
                                var name = file.Replace(".res", string.Empty);
                                if (File.Exists(enabled + $"\\{name}.res"))
                                    File.Move(enabled + $"\\{name}.res", custom + $"\\{name}.res");
                            }

                            if (!string.Equals(userSetting.Value, "0")) // Move to customization folder
                            {
                                var item = control.Options[int.Parse(userSetting.Value)];
                                if (item.FileName is not null)
                                {
                                    var file = item.FileName.Replace(".res", string.Empty);
                                    if (File.Exists(custom + $"\\{file}.res"))
                                        File.Move(custom + $"\\{file}.res", enabled + $"\\{file}.res");
                                }
                            }

                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        ///     Write user selected options to HUD files.
        /// </summary>
        /// <param name="hudSetting">Settings as defined for the HUD</param>
        /// <param name="userSetting">Settings as selected by the user</param>
        /// <param name="hudFolders">folders/files/properties Dictionary to write HUD properties to</param>
        private void WriteToFile(Controls hudSetting, Setting userSetting, Dictionary<string, dynamic> hudFolders)
        {
            try
            {
                // Check for special case conditions, namely toggling stock backgrounds.
                EnableStockBackgrounds(hudSetting, userSetting);

                if (hudSetting.Files == null) return;

                Dictionary<string, dynamic> CompileHudElement(JObject element, string filePath)
                {
                    var hudElement = new Dictionary<string, dynamic>();
                    foreach (var property in element)
                        if (string.Equals(property.Key, "replace", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var values = property.Value.ToArray();

                            string find, replace;
                            if (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                find = values[0].ToString();
                                replace = values[1].ToString();
                            }
                            else
                            {
                                find = values[1].ToString();
                                replace = values[0].ToString();
                            }

                            File.WriteAllText(filePath, File.ReadAllText(filePath).Replace(find, replace));
                        }
                        else if (property.Value.GetType() == typeof(JObject))
                        {
                            var currentObj = property.Value.ToObject<JObject>();

                            if (currentObj.ContainsKey("true") && currentObj.ContainsKey("false"))
                                hudElement[property.Key] = currentObj[userSetting.Value.ToLowerInvariant()];
                            else
                                hudElement[property.Key] = CompileHudElement(currentObj, filePath);
                        }
                        else
                        {
                            if (string.Equals(userSetting.Type, "ColorPicker", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // If the color is supposed to have a pulse, set the pulse value in the schema.
                                if (hudSetting.Pulse)
                                {
                                    var pulseKey = property.Key + "Pulse";
                                    hudElement[pulseKey] = Utilities.GetPulsedColor(userSetting.Value);
                                }

                                // If the color value is for an item rarity, update the dimmed and grayed values.
                                foreach (var value in Utilities.itemRarities)
                                {
                                    if (!string.Equals(property.Key, value.Item1)) continue;
                                    hudElement[value.Item2] = Utilities.GetDimmedColor(userSetting.Value);
                                    hudElement[value.Item3] = Utilities.GetGrayedColor(userSetting.Value);
                                }
                            }

                            hudElement[property.Key] =
                                property.Value.ToString().Replace("$value", userSetting.Value);
                        }

                    return hudElement;
                }

                void WriteAnimationCustomizations(string filePath, JObject animationOptions)
                {
                    // Don't read animations file unless the user requests a new event
                    // the majority of the animation customisations are for enabling/disabling
                    // events, which use the 'replace' keyword
                    Dictionary<string, List<HUDAnimation>> animations = null;

                    foreach (var animationOption in animationOptions)
                        switch (animationOption.Key.ToLowerInvariant())
                        {
                            case "replace":
                            {
                                // Example:
                                // "replace": [
                                //   "HudSpyDisguiseFadeIn_disabled",
                                //   "HudSpyDisguiseFadeIn"
                                // ]

                                var values = animationOption.Value.ToArray();

                                string find, replace;
                                if (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    find = values[0].ToString();
                                    replace = values[1].ToString();
                                }
                                else
                                {
                                    find = values[1].ToString();
                                    replace = values[0].ToString();
                                }

                                File.WriteAllText(filePath, File.ReadAllText(filePath).Replace(find, replace));
                                break;
                            }
                            case "comment":
                            {
                                // Example:
                                // "comment": [
                                //   "StopEvent",
                                //   "StopEvent"
                                // ]

                                var values = animationOption.Value.ToArray();

                                if (bool.TryParse(userSetting.Value, out var valid))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetStringIndexes(lines, value))
                                        lines[index] = valid
                                            ? Utilities.CommentOutTextLine(lines[index])
                                            : Utilities.UncommentOutTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetStringIndexes(lines, value))
                                        lines[index] = Utilities.CommentOutTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }

                                break;
                            }
                            case "uncomment":
                            {
                                // Example:
                                // "uncomment": [
                                //   "StopEvent",
                                //   "StopEvent"
                                // ]

                                var values = animationOption.Value.ToArray();

                                if (bool.TryParse(userSetting.Value, out var valid))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetStringIndexes(lines, value))
                                        lines[index] = valid
                                            ? Utilities.UncommentOutTextLine(lines[index])
                                            : Utilities.CommentOutTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetStringIndexes(lines, value))
                                        lines[index] = Utilities.UncommentOutTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }

                                break;
                            }
                            default:
                            {
                                // animation
                                // example:
                                // "HudHealthBonusPulse": [
                                //   {
                                //     "Type": "Animate",
                                //     "Element": "PlayerStatusHealthValue",
                                //     "Property": "Fgcolor",
                                //     "Value": "0 170 255 255",
                                //     "Interpolator": "Linear",
                                //     "Delay": "0",
                                //     "Duration": "0"
                                //   }
                                // ]

                                if (animations == null) animations = HUDAnimations.Parse(File.ReadAllText(filePath));

                                // Create new event or animation statements could stack
                                // over multiple 'apply customisations'
                                animations[animationOption.Key] = new List<HUDAnimation>();

                                foreach (var _animation in animationOption.Value.ToArray())
                                {
                                    var animation = _animation.ToObject<Dictionary<string, dynamic>>();

                                    // Create temporary variable to store current animation instead of adding directly in switch case
                                    // because there are conditional properties that might need to be added later
                                    //
                                    // Initialize to dynamic so type checker doesnt return HUDAnimation
                                    // for setting freq/gain/bias
                                    //
                                    dynamic current = null;

                                    switch (animation["Type"].ToString().ToLower())
                                    {
                                        case "animate":
                                            current = new Animate
                                            {
                                                Type = "Animate",
                                                Element = animation["Element"],
                                                Property = animation["Property"],
                                                Value = animation["Value"],
                                                Interpolator = animation["Interpolator"],
                                                Delay = animation["Delay"],
                                                Duration = animation["Duration"]
                                            };
                                            break;

                                        case "runevent":
                                            current = new RunEvent
                                            {
                                                Type = "RunEvent",
                                                Event = animation["Event"],
                                                Delay = animation["Delay"]
                                            };
                                            break;

                                        case "stopevent":
                                            current = new StopEvent
                                            {
                                                Type = "StopEvent",
                                                Event = animation["Event"],
                                                Delay = animation["Delay"]
                                            };
                                            break;

                                        case "setvisible":
                                            current = new SetVisible
                                            {
                                                Type = "StopEvent",
                                                Element = animation["Element"],
                                                Delay = animation["Delay"],
                                                Duration = animation["Duration"]
                                            };
                                            break;

                                        case "firecommand":
                                            current = new FireCommand
                                            {
                                                Type = "FireCommand",
                                                Delay = animation["Delay"],
                                                Command = animation["Command"]
                                            };
                                            break;

                                        case "runeventchild":
                                            current = new RunEventChild
                                            {
                                                Type = "RunEventChild",
                                                Element = animation["Element"],
                                                Event = animation["Event"],
                                                Delay = animation["Delay"]
                                            };
                                            break;

                                        case "setinputenabled":
                                            current = new SetInputEnabled
                                            {
                                                Type = "SetInputEnabled",
                                                Element = animation["Element"],
                                                Visible = animation["Visible"],
                                                Delay = animation["Delay"]
                                            };
                                            break;

                                        case "playsound":
                                            current = new PlaySound
                                            {
                                                Type = "PlaySound",
                                                Delay = animation["Delay"],
                                                Sound = animation["Sound"]
                                            };
                                            break;

                                        case "stoppanelanimations":
                                            current = new StopPanelAnimations
                                            {
                                                Type = "StopPanelAnimations",
                                                Element = animation["Element"],
                                                Delay = animation["Delay"]
                                            };
                                            break;

                                        default:
                                            throw new Exception(
                                                $"Unexpected animation type '{animation["Type"]}' in {animationOption.Key}!");
                                    }

                                    // Animate statements can have an extra argument make sure to account for them
                                    if (current.GetType() == typeof(Animate))
                                    {
                                        if (string.Equals(current.Interpolator, "pulse", StringComparison.CurrentCultureIgnoreCase))
                                            current.Frequency = animation["Frequency"];

                                        if (string.Equals(current.Interpolator, "gain", StringComparison.CurrentCultureIgnoreCase) ||
                                            string.Equals(current.Interpolator, "bias", StringComparison.CurrentCultureIgnoreCase))
                                            current.Bias = animation["Bias"];
                                    }

                                    animations[animationOption.Key].Add(current);
                                }

                                break;
                            }
                        }

                    if (animations != null) File.WriteAllText(filePath, HUDAnimations.Stringify(animations));
                }

                string[] resFileExtensions = {"res", "vmt", "vdf"};

                foreach (var filePath in hudSetting.Files)
                {
                    var currentFilePath = MainWindow.HudPath + "\\" + Name + "\\" + filePath.Key;
                    var extension = filePath.Key.Split(".")[^1];

                    if (resFileExtensions.Contains(extension))
                    {
                        var hudFile = Utilities.CreateNestedObject(hudFolders, Regex.Split(filePath.Key, @"[\/]+"));
                        Utilities.Merge(hudFile,
                            CompileHudElement(filePath.Value.ToObject<JObject>(),
                                currentFilePath));
                    }
                    else if (string.Equals(extension, "txt"))
                    {
                        // assume .txt is always an animation file
                        // (may cause issues with mod_textures.txt but assume we are only editing hud files)
                        WriteAnimationCustomizations(currentFilePath,
                            filePath.Value.ToObject<JObject>());
                    }
                    else
                    {
                        MessageBox.Show($"Could not recognise file extension '{extension}'",
                            "Unrecognized file extension");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void EnableStockBackgrounds(Controls hudSetting, Setting userSetting)
        {
            // Check for special conditions, namely if we should enable stock backgrounds.
            var enable = string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase);

            if (string.Equals(hudSetting.Type, "ComboBox", StringComparison.CurrentCultureIgnoreCase))
            {
                // Determine files using the files of the selected item's label or value
                // Could cause issues if label and value are both numbers but numbered differently
                hudSetting.Files = hudSetting.Options.First(x => x.Label == userSetting.Value || x.Value == userSetting.Value).Files;

                foreach (var option in hudSetting.Options)
                    if (option.Special is not null)
                    {
                        hudSetting.Special = option.Special;
                        if (option.Value == userSetting.Value)
                            enable = true;
                    }
            }

            if (hudSetting.Special is null) return;

            if (string.Equals(hudSetting.Special, "StockBackgrounds", StringComparison.CurrentCultureIgnoreCase))
                custom.SetStockBackgrounds(MainWindow.HudPath + "\\" + Name + "\\materials\\console", enable);
        }
    }
}