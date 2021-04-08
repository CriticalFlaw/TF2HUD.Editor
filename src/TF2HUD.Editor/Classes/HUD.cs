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
        public Dictionary<string, Type> ConstrolList = new();
        public Dictionary<string, Controls[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
        public string Default;
        public string EnabledFolder;
        private string[][] Layout;
        public string Name;
        public string UpdateUrl, GitHubUrl, HudsTfUrl, SteamUrl, IssueUrl;

        public HUD(string name, HudJson options)
        {
            // Validate properties from JSON
            Name = name;
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
        }

        /// <summary>
        ///     Generate the page layout using controls defined in the HUD's JSON.
        /// </summary>
        public Grid GetControls()
        {
            SetupUserSettings();

            if (ControlsRendered)
            {
                Load();
                return Controls;
            }

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
            var sectionsContainer = new Grid
            {
                VerticalAlignment = VerticalAlignment.Top,
                MaxWidth = 1270,
                MaxHeight = 720
            };

            if (LayoutOptions is null)
            {
                Layout = new string[4][];
                Layout[0] = new[] {"0", "0", "0", "1", "1", "1"};
                Layout[1] = new[] {"2", "2", "3", "4", "4", "4"};
                Layout[2] = new[] {"5", "5", "5", "5", "4", "4"};
                Layout[3] = new[] {"6", "6", "6", "6", "4", "4"};
            }
            else
            {
                // Splits Layout string[] into 2D Array using \s+
                Layout = LayoutOptions.Select(t => Regex.Split(t, "\\s+")).ToArray();
            }

            // Assume that all row arrays are the same length, use column information from Layout[0]
            for (var i = 0; i < Layout[0].Length; i++)
                sectionsContainer.ColumnDefinitions.Add(new ColumnDefinition());
            for (var i = 0; i < Layout.Length; i++) sectionsContainer.RowDefinitions.Add(new RowDefinition());
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
                            charContainer.Children.Add(charLabel);
                            charContainer.Children.Add(charInput);
                            sectionContent.Children.Add(charContainer);
                            ConstrolList.Add(charInput.Name, charInput.GetType());
                            break;

                        case "Checkbox":
                            var checkBoxInput = new CheckBox
                            {
                                Name = id,
                                Content = label,
                                Margin = new Thickness(10, lastTop + 10, 0, 0),
                                IsChecked = string.Equals(controlItem.Default, "true")
                            };
                            //lastMargin = checkBoxInput.Margin;
                            sectionContent.Children.Add(checkBoxInput);
                            ConstrolList.Add(checkBoxInput.Name, checkBoxInput.GetType());
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
                                colorInput.SelectedColor =
                                    (Color) new ColorConverter().ConvertFrom(controlItem.Default);
                            }
                            catch
                            {
                                colorInput.SelectedColor = Color.FromArgb(255, 0, 255, 0);
                            }

                            colorContainer.Children.Add(colorLabel);
                            colorContainer.Children.Add(colorInput);
                            sectionContent.Children.Add(colorContainer);
                            ConstrolList.Add(colorInput.Name, colorInput.GetType());
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
                                SelectedIndex = int.TryParse(controlItem.Default, out var index) ? index : 1
                            };
                            // TODO: Add the display value and actual value.
                            //var OptionValue = option.Value;
                            foreach (var option in controlItem.Options)
                            {
                                var item = new ComboBoxItem
                                {
                                    Content = option.Label
                                };
                                if (string.Equals(controlItem.Type, "Crosshair"))
                                {
                                    comboBoxInput.Style = (Style) Application.Current.Resources["CrosshairBox"];
                                    item.Style = (Style) Application.Current.Resources["Crosshair"];
                                }

                                comboBoxInput.Items.Add(item);
                            }

                            comboBoxContainer.Children.Add(comboBoxLabel);
                            comboBoxContainer.Children.Add(comboBoxInput);
                            sectionContent.Children.Add(comboBoxContainer);
                            ConstrolList.Add(comboBoxInput.Name, comboBoxInput.GetType());
                            break;

                        case "Number":
                            var numberContainer = new WrapPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 0)
                            };
                            var numberLabel = new Label
                            {
                                Content = label,
                                Width = 60
                            };
                            var numberInput = new TextBox
                            {
                                Name = id,
                                Width = 60
                            };
                            numberInput.PreviewTextInput += (_, e) => { e.Handled = !Regex.IsMatch(e.Text, "\\d"); };
                            numberContainer.Children.Add(numberLabel);
                            numberContainer.Children.Add(numberInput);
                            sectionContent.Children.Add(numberContainer);
                            ConstrolList.Add(numberInput.Name, numberInput.GetType());
                            break;

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
                                Value = int.TryParse(controlItem.Default, out index) ? index : 0,
                                Minimum = controlItem.Minimum,
                                Maximum = controlItem.Maximum,
                                Increment = controlItem.Increment
                            };
                            integerContainer.Children.Add(integerLabel);
                            integerContainer.Children.Add(integerInput);
                            sectionContent.Children.Add(integerContainer);
                            ConstrolList.Add(integerInput.Name, integerInput.GetType());
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
        ///     Save user-settings to a Json file.
        /// </summary>
        public void Save()
        {
            try
            {
                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is StackPanel
                                stackPanel)
                                switch ((Visual) VisualTreeHelper.GetChild(stackPanel, stackPanel.Children.Count - 1))
                                {
                                    case TextBox text:
                                        UpdateJson(text.Name, text.Text);
                                        break;

                                    case ColorPicker color:
                                        UpdateJson(color.Name, Utilities.RgbConverter(color.SelectedColor.ToString()));
                                        break;

                                    case ComboBox combo:
                                        UpdateJson(combo.Name,
                                            ((ComboBoxItem) combo.SelectedItem).Style ==
                                            (Style) Application.Current.Resources["Crosshair"]
                                                ? combo.SelectedValue.ToString()
                                                : combo.SelectedIndex.ToString());
                                        break;

                                    case IntegerUpDown integer:
                                        UpdateJson(integer.Name, integer.Text);
                                        break;
                                }
                            else if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is CheckBox
                                checkBox) UpdateJson(checkBox.Name, checkBox.IsChecked == true ? "true" : "false");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Load user-settings from a Json file.
        /// </summary>
        public void Load()
        {
            try
            {
                // TODO: If the json has a duplicate key, this process fails. The json will need to be validated before we get to this point.
                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is StackPanel
                                stackPanel)
                            {
                                var control =
                                    (Visual) VisualTreeHelper.GetChild(stackPanel, stackPanel.Children.Count - 1);
                                switch (control)
                                {
                                    case TextBox text:
                                        text.Text = ReadFromJson(text.Name, control);
                                        break;

                                    case ColorPicker color:
                                        color.SelectedColor = ReadFromJson(color.Name, control);
                                        break;

                                    case ComboBox combo:
                                        if (((ComboBoxItem) combo.Items[0]).Style ==
                                            (Style) Application.Current.Resources["Crosshair"])
                                            combo.SelectedValue = ReadFromJson(combo.Name, control, true).ToString();
                                        else
                                            combo.SelectedIndex = ReadFromJson(combo.Name, control);
                                        break;

                                    case IntegerUpDown integer:
                                        integer.Text = ReadFromJson(integer.Name, control);
                                        break;
                                }
                            }
                            else if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is CheckBox
                                checkBox)
                            {
                                checkBox.IsChecked = ReadFromJson(checkBox.Name, checkBox);
                            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Reset user-settings to the default as defined in the HUD's Json file.
        /// </summary>
        public void Reset()
        {
            try
            {
                var json = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                    .Controls;
                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is StackPanel
                                stackPanel)
                            {
                                var control =
                                    (Visual) VisualTreeHelper.GetChild(stackPanel, stackPanel.Children.Count - 1);
                                switch (control)
                                {
                                    case TextBox text:
                                        text.Text = GetDefaultFromControls(text.Name, json);
                                        break;

                                    case ColorPicker color:
                                        var colors = Array.ConvertAll(
                                            GetDefaultFromControls(color.Name, json).Split(' '), c => byte.Parse(c));
                                        color.SelectedColor =
                                            Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
                                        break;

                                    case ComboBox combo:
                                        if (((ComboBoxItem) combo.Items[0]).Style ==
                                            (Style) Application.Current.Resources["Crosshair"])
                                            combo.SelectedValue = GetDefaultFromControls(combo.Name, json);
                                        else
                                            combo.SelectedIndex = int.Parse(GetDefaultFromControls(combo.Name, json));
                                        break;

                                    case IntegerUpDown integer:
                                        integer.Text = GetDefaultFromControls(integer.Name, json);
                                        break;
                                }
                            }
                            else if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is CheckBox
                                checkBox)
                            {
                                checkBox.IsChecked = Utilities.ParseBool(GetDefaultFromControls(checkBox.Name, json));
                            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Retrieve the default value defined for a given control.
        /// </summary>
        public string GetDefaultFromControls(string name, Dictionary<string, Controls[]> controls)
        {
            foreach (var collection in controls.Values)
            foreach (var control in collection)
                if (string.Equals(name, control.Name))
                    return control.Default;
            return null;
        }

        /// <summary>
        ///     Save a value to the user-settings Json file.
        /// </summary>
        public bool UpdateJson(string key, string value)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText("settings.json"));
                json.Settings.Where(x => x.HUD == Name).First(x => x.Name == key).Value = value;
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(json, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        ///     Retrieve a value from the user-settings Json file.
        /// </summary>
        public dynamic ReadFromJson(string key, Visual control, bool returnVal = false)
        {
            try
            {
                SetupUserSettings();
                var json = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText("settings.json"));
                var value = json.Settings.Where(x => x.HUD == Name).First(x => x.Name == key).Value;
                if (returnVal) return value;
                switch (control)
                {
                    case CheckBox:
                        return value == "true" || value == "1";

                    case ColorPicker:
                        var colors = Array.ConvertAll(value.Split(' '), c => byte.Parse(c));
                        return Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);

                    case ComboBox:
                        return int.Parse(value);

                    default: //TextBox, IntegerUpDown
                        return value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Retrieve a value from the user-settings Json file.
        /// </summary>
        public void SetupUserSettings()
        {
            try
            {
                // Check if the user settings file is present, if not create it.
                if (!File.Exists("settings.json"))
                    File.Create("settings.json");

                // Check if the file has HUD's settings, if not recreate them.
                var hudJson = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"));
                var userJson = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText("settings.json"));
                var userSettings = new UserJson {Settings = new List<Setting>()};

                if (userJson is null)
                {
                    // File is new, generate only current HUD settings.
                    UpdateUserSettings(userSettings, hudJson.Controls);
                }
                else
                {
                    // Check if the user already has settings for the currently selected HUD, no further actions needed.
                    if (userJson.Settings.Where(x => x.HUD == Name).Any()) return;

                    // Collect settings for other HUDs, retain them.
                    foreach (var setting in userJson.Settings.Where(x => x.HUD != Name))
                        userSettings.Settings.Add(setting);

                    // File is old but may not have the options for the selected HUD.
                    UpdateUserSettings(userSettings, hudJson.Controls);
                }

                // TODO: Check that the current version of user settings matches the latest HUD schema
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateUserSettings(UserJson userSettings, Dictionary<string, Controls[]> controlGroups)
        {
            // File is new, add just the current HUD settings.
            foreach (var group in controlGroups)
            foreach (var control in group.Value)
            {
                var user = new Setting
                {
                    HUD = Name,
                    Name = control.Name,
                    Type = control.Type,
                    Value = control.Default
                };
                userSettings.Settings.Add(user);
            }

            // Clear the file before reapplying the settings.
            File.WriteAllText("settings.json", string.Empty);
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(userSettings, Formatting.Indented));
        }

        /// <summary>
        ///     Apply user-set customizations to the HUD files.
        /// </summary>
        public bool ApplyCustomization()
        {
            try
            {
                var path = $"{MainWindow.HudPath}\\{Name}\\";

                // If the developer defined customization folders for their HUD, then copy those files.
                if (!string.IsNullOrWhiteSpace(CustomisationsFolder)) MoveCustomizationFiles(path);

                var userSettings = JsonConvert
                    .DeserializeObject<UserJson>(File.ReadAllText("settings.json")).Settings
                    .Where(x => x.HUD == Name);
                var hudSettings = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                    .Controls.Values;

                // This Dictionary contains folders/files/properties as they should be written to the hud
                // the 'IterateFolder' and 'IterateHUDFileProperties' will write the properties to this
                var hudFolders = new Dictionary<string, dynamic>();

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    var user = userSettings.First(x => x.Name == control.Name);
                    if (user is not null)
                        WriteToFile(path, control, user, hudFolders);
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
        public bool MoveCustomizationFiles(string path)
        {
            try
            {
                // Check if the customization folders are valid.
                if (!Directory.Exists($"{path}\\{CustomisationsFolder}")) return true;

                var controlGroups = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                    .Controls.Values;
                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            switch ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y))
                            {
                                case CheckBox check:
                                    var fileName = Utilities.GetFileName(controlGroups, check.Name);
                                    if (string.IsNullOrWhiteSpace(fileName)) continue; // File name not found, skipping.
                                    var custom = $"{path}{CustomisationsFolder}\\{fileName}.res";
                                    var enabled = $"{path}{EnabledFolder}\\{fileName}.res";

                                    if (check.IsChecked == true) // Move to enabled
                                    {
                                        if (File.Exists(custom)) File.Move(custom, enabled);
                                    }
                                    else // Move to customization folder
                                    {
                                        if (File.Exists(enabled)) File.Move(enabled, custom);
                                    }

                                    break;
                                case ComboBox combo:
                                    // TODO: Add ComboBox control option
                                    break;
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
        /// <param name="path">Path to the HUD installation</param>
        /// <param name="hudSetting">Settings as defined for the HUD</param>
        /// <param name="userSetting">Settings as selected by the user</param>
        /// <param name="hudFolders">folders/files/properties Dictionary to write HUD properties to</param>
        private void WriteToFile(string path, Controls hudSetting, Setting userSetting,
            Dictionary<string, dynamic> hudFolders)
        {
            try
            {
                if (hudSetting.Special is not null)
                    if (string.Equals(hudSetting.Special, "StockBackgrounds"))
                        custom.SetStockBackgrounds(MainWindow.HudPath + "\\" + Name + "\\materials\\console",
                            userSetting.Value == "true");

                if (hudSetting.Type == "ComboBox")
                    hudSetting.Files = hudSetting.Options.Where(x => x.Value == userSetting.Value).First().Files;

                if (hudSetting.Files != null)
                {
                    Dictionary<string, dynamic> CompileHudElement(JObject element, string filePath)
                    {
                        var hudElement = new Dictionary<string, dynamic>();
                        foreach (var property in element)
                            if (property.Key == "replace")
                            {
                                var values = property.Value.ToArray();

                                string find, replace;
                                if (userSetting.Value == "true")
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
                                    hudElement[property.Key] = currentObj[userSetting.Value];
                                else
                                    hudElement[property.Key] = CompileHudElement(currentObj, filePath);
                            }
                            else
                            {
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
                            if (animationOption.Key == "replace")
                            {
                                // Example:
                                // "replace": [
                                //   "HudSpyDisguiseFadeIn_disabled",
                                //   "HudSpyDisguiseFadeIn"
                                // ]

                                var values = animationOption.Value.ToArray();

                                string find, replace;
                                if (userSetting.Value == "true")
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
                            else if (animationOption.Key == "comment")
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
                            }
                            else if (animationOption.Key == "uncomment")
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
                            }
                            else
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
                                        if (current.Interpolator.ToLower() == "pulse")
                                            current.Frequency = animation["Frequency"];

                                        if (current.Interpolator.ToLower() == "gain" ||
                                            current.Interpolator.ToLower() == "bias")
                                            current.Bias = animation["Bias"];
                                    }

                                    animations[animationOption.Key].Add(current);
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
                        else if (extension == "txt")
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}