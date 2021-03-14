using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using TF2HUD.Editor.JSON;
using Xceed.Wpf.Toolkit;

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
                sectionContent.Margin = new Thickness(5);

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
                            foreach (var option in controlItem.Options)
                                // TOOD: Add the display value and actual value.
                                //var OptionValue = option.Value;
                                comboBoxInput.Items.Add(option.Label);

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
                                        UpdateJson(color.Name, color.SelectedColor.ToString());
                                        break;

                                    case ComboBox combo:
                                        UpdateJson(combo.Name, combo.SelectedIndex.ToString());
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
                                        var cc = new ColorConverter();
                                        color.SelectedColor =
                                            (Color) cc.ConvertFrom(GetDefaultFromControls(color.Name, json));
                                        break;

                                    case ComboBox combo:
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
        public dynamic ReadFromJson(string key, Visual control)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText("settings.json"));
                var value = json.Settings.Where(x => x.HUD == Name).First(x => x.Name == key).Value;
                switch (control)
                {
                    case CheckBox:
                        return value == "true" || value == "1";

                    case ColorPicker:
                        var cc = new ColorConverter();
                        return (Color) cc.ConvertFrom(value);

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
        ///     Apply user-set customizations to the HUD files.
        /// </summary>
        public bool ApplyCustomization()
        {
            try
            {
                var path = $"{MainWindow.HudPath}\\{Name}\\";

                // If the developer defined customization folders for their HUD, then copy those files.
                if (!string.IsNullOrWhiteSpace(CustomisationsFolder)) return MoveCustomizationFiles(path);

                var userSettings = JsonConvert
                    .DeserializeObject<UserJson>(File.ReadAllText("settings.json")).Settings
                    .Where(x => x.HUD == Name);
                var hudSettings = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                    .Controls.Values;

                // This Dictionary contains folders/files/properties as they should be written to the hud
                // the 'IterateFolder' and 'IterateHUDFileProperties' will write the properties to this
                var hudFolders = new Dictionary<string, dynamic>();

                foreach (var userSetting in userSettings)
                foreach (var control in from hudSetting in hudSettings
                    from control in hudSetting
                    where string.Equals(control.Name, userSetting.Name)
                    select control)
                    WriteToFile(path, control, userSetting, hudFolders);

                void IterateProperties(Dictionary<string, dynamic> folder, string folderPath)
                {
                    foreach (string property in folder.Keys)
                    {
                        if (folder[property].GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            if (property.Contains("."))
                            {
                                // Merge files
                                string filePath = folderPath + "\\" + property;
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                                if (File.Exists(filePath))
                                {
                                    var obj = VDF.Parse(File.ReadAllText(filePath));
                                    Utilities.Merge(obj, folder[property]);
                                    File.WriteAllText(filePath, VDF.Stringify(obj));
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
                }

                // write hudFolders to the HUD once instead of each WriteToFile call reading and writing
                IterateProperties(hudFolders, MainWindow.HudPath + "\\" + Name);

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
                if (Directory.Exists($"{path}\\{CustomisationsFolder}")) return true;

                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is CheckBox checkBox
                            )
                            {
                                var custom = $"{path}{CustomisationsFolder}\\{checkBox.Name}.res";
                                var enabled = $"{path}{EnabledFolder}\\{checkBox.Name}.res";
                                if (checkBox.IsChecked == true) // Move to enabled
                                {
                                    if (File.Exists(custom)) File.Move(custom, enabled);
                                }
                                else // Move to customization folder
                                {
                                    if (File.Exists(enabled)) File.Move(enabled, custom);
                                }

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
        private void WriteToFile(string path, Controls hudSetting, Setting userSetting, Dictionary<string, dynamic> hudFolders)
        {
            try
            {
                if (hudSetting.Instruction != null)
                {
                    void IterateFolder(Newtonsoft.Json.Linq.JObject folder, string folderPath, Dictionary<string, dynamic> hudFolder)
                    {
                        foreach (var item in folder)
                        {
                            if (item.Value.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                            {
                                if (!hudFolder.ContainsKey(item.Key))
                                    hudFolder[item.Key] = new Dictionary<string, dynamic>();

                                if (item.Key.Contains("."))
                                {
                                    string extension = item.Key.Split('.')[^1];
                                    switch (extension)
                                    {
                                        case "res":
                                            IterateHUDFileProperties(item.Value.ToObject<Newtonsoft.Json.Linq.JObject>(), item.Key, folderPath + "\\" + item.Key, hudFolder[item.Key]);
                                            break;
                                        case "vmt":
                                        case "vdf":
                                            IterateHUDFileProperties(item.Value.ToObject<Newtonsoft.Json.Linq.JObject>(), item.Key, folderPath + "\\" + item.Key, hudFolder[item.Key]);
                                            break;
                                        default:
                                            System.Windows.MessageBox.Show($"Could not recognise file extension '{extension}'");
                                            break;
                                    }
                                }
                                else
                                {
                                    IterateFolder(item.Value.ToObject<Newtonsoft.Json.Linq.JObject>(), folderPath + "\\" + item.Key, hudFolder[item.Key]);
                                }
                            }
                            else
                            {
                                throw new Exception($"Unexpected type {item.Value.GetType().Name}");
                            }
                        }
                    }

                    void IterateHUDFileProperties(Newtonsoft.Json.Linq.JObject properties, string propertyName, string filePath, Dictionary<string, dynamic> hudFolder)
                    {
                        foreach (var item in properties)
                        {
                            if (item.Key == "replace")
                            {
                                Newtonsoft.Json.Linq.JToken[] values = item.Value.ToArray();

                                string find, replace;
                                if (userSetting.Value.ToString() == "true")
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
                            else if (item.Value.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                            {
                                var currentObj = item.Value.ToObject<Newtonsoft.Json.Linq.JObject>();

                                if (currentObj.ContainsKey("true") && currentObj.ContainsKey("false"))
                                {
                                    hudFolder[item.Key] = currentObj[userSetting.Value.ToString()];
                                }
                                else
                                {
                                    if (!hudFolder.ContainsKey(item.Key))
                                        hudFolder[item.Key] = new Dictionary<string, dynamic>();

                                    IterateHUDFileProperties(currentObj, item.Key, filePath, hudFolder[item.Key]);
                                }
                            }
                            else
                                hudFolder[item.Key] = item.Value.ToString().Replace("$value", userSetting.Value);
                        }
                    }

                    IterateFolder(hudSetting.Instruction, path, hudFolders);

                    return;
                }


                foreach (var instruction in hudSetting.Instructions.OrEmptyIfNull())
                {
                    var res = path + instruction.FileName;
                    if (!File.Exists(res)) continue;

                    var start = 0;
                    var lines = File.ReadAllLines(res);
                    if (!string.IsNullOrWhiteSpace(instruction.Group))
                        start = Utilities.FindIndex(lines, $"\"{instruction.Group}\"");

                    switch (instruction.Group)
                    {
                        case "DamagedPlayer":
                            stock.CrosshairPulse(res, bool.Parse(userSetting.Value));
                            break;

                        case "HudSpyDisguiseFadeIn":
                            stock.DisguiseImage(res, bool.Parse(userSetting.Value));
                            break;

                        case "TransparentViewmodel":
                            stock.TransparentViewmodels(res, bool.Parse(userSetting.Value));
                            break;

                        case "HudDeathNotice":
                            stock.KillFeedRows(res, int.Parse(userSetting.Value));
                            break;

                        case "TFCharacterImage":
                            stock.MainMenuClassImage(res, bool.Parse(userSetting.Value),
                                int.Parse(instruction.Values.FirstOrDefault()));
                            break;

                        default:
                            switch (hudSetting.Type)
                            {
                                case "Checkbox":
                                    for (var x = 0; x < instruction.Tags.Length; x++)
                                        if (!string.IsNullOrWhiteSpace(instruction.Command))
                                            switch (instruction.Command.ToLowerInvariant())
                                            {
                                                case "replace":
                                                    var value = string.Equals(userSetting.Value, "true")
                                                        ? instruction.Values[x]
                                                        : instruction.Defaults[x];
                                                    lines[
                                                            Utilities.FindIndex(lines, $"\"{instruction.Tags[x]}\"",
                                                                start)]
                                                        = $"\t\t\"{instruction.Tags[x]}\"\t\t\t\t\t\"{value}\"";
                                                    break;

                                                case "comment":
                                                    stock.DisguiseImage(res, bool.Parse(userSetting.Value));
                                                    break;

                                                default:
                                                    lines[Utilities.FindIndex(lines, instruction.Tags[x], start)] =
                                                        string.Equals(userSetting.Value, "true")
                                                            ? instruction.Values[x]
                                                            : instruction.Defaults[x];
                                                    break;
                                            }

                                    break;

                                case "Color":
                                case "Colour":
                                case "ColorPicker":
                                case "ColourPicker":
                                    foreach (var tag in instruction.Tags)
                                        lines[Utilities.FindIndex(lines, $"\"{tag}\"", start)] =
                                            $"\t\t\"{tag}\"\t\t\t\t\t\"{Utilities.RgbConverter(userSetting.Value)}\"";
                                    break;

                                case "DropDown":
                                case "DropDownMenu":
                                case "Select":
                                case "ComboBox":
                                    break;

                                case "IntegerUpDown":
                                    foreach (var tag in instruction.Tags)
                                        lines[Utilities.FindIndex(lines, $"\"{tag}\"", start)] =
                                            $"\t\t\"{tag}\"\t\t\t\t\t\"{userSetting.Value}\"";
                                    break;
                            }

                            File.WriteAllLines(res, lines);
                            break;
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