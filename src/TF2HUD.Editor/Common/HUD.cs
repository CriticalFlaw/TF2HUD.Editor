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
        public Dictionary<string, Type> ConstrolList = new();
        public Dictionary<string, Control[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
        public string Default;
        public string EnabledFolder;
        private string[][] Layout;
        private readonly string[] LayoutOptions;
        public string Name;
        public string UpdateUrl;

        public HUD(string name, HUDRoot options)
        {
            // Validate properties from JSON
            this.Name = name;
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

            if (LayoutOptions != null)
                // Splits Layout string[] into 2D Array using \s+
                Layout = LayoutOptions.Select(t => Regex.Split(t, "\\s+")).ToArray();

            // ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic
            dynamic sectionsContainer;
            if (Layout != null)
            {
                sectionsContainer = new Grid();
                // Assume that all row arrays are the same length, use column information from Layout[0]
                for (var i = 0; i < Layout[0].Length; i++)
                    sectionsContainer.ColumnDefinitions.Add(new ColumnDefinition());
                for (var i = 0; i < Layout.Length; i++) sectionsContainer.RowDefinitions.Add(new RowDefinition());
                sectionsContainer.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                // If no layout is provided, wrap GroupPanels to space
                sectionsContainer = new WrapPanel();
                sectionsContainer.Orientation = Orientation.Vertical;
            }

            sectionsContainer.MaxWidth = 1270;
            sectionsContainer.MaxHeight = 720;
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
                    var id = controlItem.Tag;
                    var label = controlItem.Label;
                    var type = controlItem.Type;
                    //var file = ControlItem.File;
                    var value = controlItem.Default;

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
                            charInput.PreviewTextInput += (sender, e) => e.Handled = charInput.Text != "";
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
                                IsChecked = controlItem.Default == "1"
                            };
                            lastMargin = checkBoxInput.Margin;
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
                                SelectedIndex = int.TryParse(controlItem.Default, out var index) ? index : 1
                            };
                            foreach (var option in controlItem.Options)
                            {
                                // TOOD: Add the display value and actual value.
                                var OptionLabel = option.Label;
                                var OptionValue = option.Value;
                                comboBoxInput.Items.Add(OptionLabel);
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
                                Content = label
                            };
                            var numberInput = new TextBox
                            {
                                Name = id,
                                Width = 60
                            };
                            numberInput.PreviewTextInput += (sender, e) =>
                            {
                                e.Handled = !Regex.IsMatch(e.Text, "\\d");
                            };
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
                var grid = (WrapPanel) ((Grid) Controls.Children[0]).Children[1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual) VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                    {
                        var panel = (StackPanel) groupBox.Content;
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount(panel); y++)
                        {
                            var control = (Visual) VisualTreeHelper.GetChild(panel, y);
                            switch (control)
                            {
                                case TextBox text:
                                    UpdateJson(text.Name, text.Text);
                                    break;
                                case CheckBox check:
                                    UpdateJson(check.Name, check.IsChecked == true ? "1" : "0");
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
                        }
                    }
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
                var grid = (WrapPanel)((Grid)Controls.Children[0]).Children[1];
                for (var x = 0; x < VisualTreeHelper.GetChildrenCount(grid); x++)
                    if ((Visual)VisualTreeHelper.GetChild(grid, x) is GroupBox groupBox)
                    {
                        var panel = (StackPanel)groupBox.Content;
                        for (var y = 0; y < VisualTreeHelper.GetChildrenCount(panel); y++)
                        {
                            var control = (Visual)VisualTreeHelper.GetChild(panel, y);
                            switch (control)
                            {
                                case TextBox text:
                                    text.Text = ReadFromJson(text.Name, control);
                                    break;
                                case CheckBox check:
                                    check.IsChecked = ReadFromJson(check.Name, control);
                                    break;
                                case ColorPicker color:
                                    color.SelectedColor = ReadFromJson(color.Name, control);
                                    break;
                                case ComboBox combo:
                                    combo.SelectedIndex = ReadFromJson(combo.Name, control); ;
                                    break;
                                case IntegerUpDown integer:
                                    integer.Text = ReadFromJson(integer.Name, control);
                                    break;
                            }
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
                        return value == "1";
                    case ColorPicker:
                        var cc = new ColorConverter();
                        return (Color)cc.ConvertFrom(value);
                    case ComboBox:
                        return int.Parse(value);
                    default:    //TextBox, IntegerUpDown
                        return value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
        [JsonPropertyName(";")] public string Label;

        [JsonPropertyName("Maximum")] public int Maximum = 30;

        [JsonPropertyName("Minimum")] public int Minimum = 10;

        [JsonPropertyName("Options")] public Option[] Options;

        [JsonPropertyName("Tag")] public string Tag;

        [JsonPropertyName("Type")] public string Type;
    }

    public class Option
    {
        [JsonPropertyName("Label")] public string Label;

        [JsonPropertyName("Value")] public string Value;
    }

    #endregion MODEL
}