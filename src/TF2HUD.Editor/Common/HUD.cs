using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit;

namespace TF2HUD.Editor.Common
{
    public class HUD
    {
        private readonly Grid Controls = new();
        private readonly string[] LayoutOptions;
        public Dictionary<string, Type> ConstrolList = new();
        public Dictionary<string, Control[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
        public string Default;
        public string EnabledFolder;
        private string[][] Layout;
        public string Name;
        public string UpdateUrl;

        public HUD(string name, HUDRoot options)
        {
            // Validate properties from JSON
            Name = name;
            UpdateUrl = !string.IsNullOrWhiteSpace(options.UpdateUrl) ? options.UpdateUrl : string.Empty;
            CustomisationsFolder = !string.IsNullOrWhiteSpace(options.CustomisationsFolder)
                ? options.CustomisationsFolder
                : string.Empty;
            EnabledFolder = !string.IsNullOrWhiteSpace(options.EnabledFolder) ? options.EnabledFolder : string.Empty;
            ControlOptions = options.Controls;
            LayoutOptions = options.Layout;
        }

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
                // TODO: Generate the layout dynamically if one was not provided.
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
                    var type = controlItem.Type;
                    var def = controlItem.Default;
                    //var file = ControlItem.File;
                    //var value = controlItem.Default;

                    switch (type)
                    {
                        case "Char":
                            var charContainer = new WrapPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 0)
                            };
                            var charLabel = new Label
                            {
                                Content = label
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
                                FontSize = 16
                            };
                            var colorInput = new ColorPicker
                            {
                                Name = id
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
                                Content = label
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
                            throw new Exception($"Type {type} is not a valid type!");
                    }

                    // lastTop = lastMargin.Top + 10;
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

        public void Update()
        {
            if (UpdateUrl != null) MainWindow.DownloadHud(UpdateUrl);
        }

        public void Save()
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

        public void Load()
        {
            try
            {
                // TODO: If the json has a duplicate key, this process fails. The json will need to be validated before we get to this point.
                // TODO: This section does not work with complex layouts because we expect a WrapPanel whereas we end up with another Grid.
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

        public void Reset()
        {
            try
            {
                var json = JsonConvert.DeserializeObject<HUDRoot>(File.ReadAllText($"Common//HUDs//{Name}.json"))
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
                                        color.SelectedColor = (Color) cc.ConvertFrom(GetDefaultFromControls(color.Name, json));
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

        public string GetDefaultFromControls(string name, Dictionary<string, Control[]> controls)
        {
            foreach (var collection in controls.Values)
                foreach (var control in collection)
                    if (string.Equals(name, control.Name))
                        return control.Default;
            return null;
        }

        public bool UpdateJson(string key, string value)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText("Common//settings.json"));
                json.Settings.Where(x => x.HUD == Name).First(x => x.Name == key).Value = value;
                File.WriteAllText("Common//settings.json", JsonConvert.SerializeObject(json, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public dynamic ReadFromJson(string key, Visual control)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText("Common//settings.json"));
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

        public bool ApplyCustomization()
        {
            try
            {
                var path = $"{MainWindow.HudPath}\\{Name}\\";
                if (string.IsNullOrWhiteSpace(CustomisationsFolder)) return ApplyFileCustomization(path);

                var json = JsonConvert.DeserializeObject<HUDRoot>(File.ReadAllText($"Common//HUDs//{Name}.json"))
                    .Controls;
                // Check if the customization folders are valid.
                var grid = (Grid) ((Grid) Controls.Children[^1]).Children[^1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount((WrapPanel) groupBox.Content); y++)
                            if ((Visual) VisualTreeHelper.GetChild((WrapPanel) groupBox.Content, y) is StackPanel
                                stackPanel)
                            {
                                var control =
                                    (Visual) VisualTreeHelper.GetChild(stackPanel, stackPanel.Children.Count - 1);
                                var instructions = new Control();
                                string name;
                                bool useDefault;
                                switch (control)
                                {
                                    case TextBox text:
                                        name = text.Name;
                                        useDefault = !string.Equals(text.Text, instructions.Default);
                                        break;

                                    case ColorPicker color:
                                        name = color.Name;
                                        useDefault = color.SelectedColor.ToString() !=
                                                     Utilities.RgbConverter(instructions.Default);
                                        break;

                                    case ComboBox combo:
                                        name = combo.Name;
                                        useDefault = combo.SelectedIndex != int.Parse(instructions.Default);
                                        break;

                                    case IntegerUpDown integer:
                                        name = integer.Name;
                                        useDefault = integer.Value != int.Parse(instructions.Default);
                                        break;

                                    case CheckBox check:
                                        name = check.Name;
                                        useDefault = check.IsChecked == true;
                                        break;

                                    default:
                                        continue;
                                }

                                foreach (var (_, variables) in json)
                                foreach (var variable in variables)
                                {
                                    if (!string.Equals(variable.Name, name)) continue;
                                    instructions = variable;
                                    break;
                                }

                                WriteToFile(path, instructions, useDefault);
                            }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool ApplyFileCustomization(string path)
        {
            try
            {
                // Check if the customization folders are valid.
                if (Directory.Exists($"{path}\\{CustomisationsFolder}")) return true;

                // TODO: If the json has a duplicate key, this process fails. The json will need to be validated before we get to this point.
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

        public static bool WriteToFile(string path, Control instructions, bool useDefault = false, string value = null)
        {
            try
            {
                foreach (var file in instructions.Instructions.OrEmptyIfNull())
                {
                    if (file is null) continue;
                    var res = path + file.FileName;
                    if (!File.Exists(res)) continue;
                    var lines = File.ReadAllLines(res);
                    var start = Utilities.FindIndex(lines, $"\"{file.Group}\"");
                    foreach (var tag in file.Tags)
                    {
                        if (!string.IsNullOrWhiteSpace(file.Value))
                            value = useDefault ? instructions.Default : file.Value;
                        else
                            value = useDefault ? instructions.Default : value;
                        lines[Utilities.FindIndex(lines, $"\"{tag}\"", start)] = $"\t\t\"{tag}\"\t\t\t\t\t\"{value}\"";
                    }

                    File.WriteAllLines(res, lines);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }

    #region MODEL

    public class HUDRoot
    {
        [JsonPropertyName("Controls")] public Dictionary<string, Control[]> Controls;

        [JsonPropertyName("CustomisationsFolder")]
        public string CustomisationsFolder;

        [JsonPropertyName("EnabledFolder")] public string EnabledFolder;
        [JsonPropertyName("Layout")] public string[] Layout;

        [JsonPropertyName("UpdateUrl")] public string UpdateUrl;
    }

    public class Control
    {
        [JsonPropertyName("Default")] public string Default = "0";

        [JsonPropertyName("Increment")] public int Increment = 2;

        [JsonPropertyName("Instructions")] public Instructions[] Instructions;

        [JsonPropertyName(";")] public string Label;

        [JsonPropertyName("Maximum")] public int Maximum = 30;

        [JsonPropertyName("Minimum")] public int Minimum = 10;
        [JsonPropertyName("Name")] public string Name;

        [JsonPropertyName("Options")] public Option[] Options;

        [JsonPropertyName("Type")] public string Type;
    }

    public class Option
    {
        [JsonPropertyName("Label")] public string Label;

        [JsonPropertyName("Value")] public string Value;
    }

    public class Instructions
    {
        [JsonPropertyName("FileName")] public string FileName;

        [JsonPropertyName("Group")] public string Group;

        [JsonPropertyName("Tags")] public string[] Tags;

        [JsonPropertyName("Value")] public string Value;
    }

    #endregion MODEL
}