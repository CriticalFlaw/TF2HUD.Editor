using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace TF2HUD.Editor.Common
{
    public class HUD
    {
        private readonly Grid Controls = new();
        private string[] LayoutOptions;
        private string[][] Layout;
        public Dictionary<string, Control[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
        public string Default;
        public string EnabledFolder;
        public string Name;
        public string UpdateUrl;

        public HUD(string Name, HUDRoot Options)
        {
            // Validate properties from JSON
            this.Name = Name;
            UpdateUrl = !string.IsNullOrWhiteSpace(Options.UpdateUrl) ? Options.UpdateUrl : string.Empty;
            CustomisationsFolder = !string.IsNullOrWhiteSpace(Options.CustomisationsFolder)
                ? Options.CustomisationsFolder
                : string.Empty;
            EnabledFolder = !string.IsNullOrWhiteSpace(Options.EnabledFolder) ? Options.EnabledFolder : string.Empty;
            ControlOptions = Options.Controls;
            LayoutOptions = Options.Layout;
        }

        public Grid GetControls()
        {
            if (ControlsRendered) return Controls;

            var Container = new Grid();
            var TitleRowDefinition = new RowDefinition();
            TitleRowDefinition.Height = GridLength.Auto;
            Container.RowDefinitions.Add(TitleRowDefinition);
            var ContentRowDefinition = new RowDefinition();
            if (Layout != null)
            {
                ContentRowDefinition.Height = GridLength.Auto;
            }
            Container.RowDefinitions.Add(ContentRowDefinition);

            // Title
            var CustomiserTitle = new Label();
            CustomiserTitle.Content = Name;
            CustomiserTitle.FontSize = 35;
            CustomiserTitle.Margin = new Thickness(10, 10, 0, 0);
            Grid.SetRow(CustomiserTitle, 0);
            Container.Children.Add(CustomiserTitle);

            if (LayoutOptions != null)
            {
                // Splits Layout string[] into 2D Array using \s+
                var TempLayout = new List<string[]>();
                for (int i = 0; i < LayoutOptions.Length; i++)
                {
                    TempLayout.Add(Regex.Split(LayoutOptions[i], "\\s+"));
                }
                Layout = TempLayout.ToArray();
            }

            // ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic
            dynamic SectionsContainer;
            if (Layout != null)
            {
                SectionsContainer = new Grid();
                // Assume that all row arrays are the same length, use column information from Layout[0]
                for (int i = 0; i < Layout[0].Length; i++)
                {
                    SectionsContainer.ColumnDefinitions.Add(new ColumnDefinition());
                }
                for (int i = 0; i < Layout.Length; i++)
                {
                    SectionsContainer.RowDefinitions.Add(new RowDefinition());
                }
                SectionsContainer.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                // If no layout is provided, wrap GroupPanels to space
                SectionsContainer = new WrapPanel();
                SectionsContainer.Orientation = Orientation.Vertical;
            }

            SectionsContainer.MaxWidth = 1270;
            SectionsContainer.MaxHeight = 720;
            Grid.SetRow(SectionsContainer, 1);

            var lastMargin = new Thickness(10, 2, 0, 0);
            var lastTop = lastMargin.Top;

            int GroupBoxIndex = 0;
            foreach (var Section in ControlOptions.Keys)
            {
                var SectionContainer = new GroupBox();
                SectionContainer.Header = Section;
                SectionContainer.Margin = new Thickness(5);
                SectionContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
                SectionContainer.VerticalAlignment = VerticalAlignment.Stretch;

                Panel SectionContent = new StackPanel();
                if (Layout != null)
                {
                    SectionContent = new WrapPanel();
                }
                SectionContent.Margin = new Thickness(5);

                foreach (var ControlItem in ControlOptions[Section])
                {
                    var Label = ControlItem.Label;
                    var Type = ControlItem.Type;
                    //var File = ControlItem.File;
                    var Default = ControlItem.Default;

                    switch (Type)
                    {
                        case "Char":
                            var CharInputContainer = new WrapPanel();
                            CharInputContainer.Margin = new Thickness(10, lastTop, 0, 0);
                            var CharInputLabel = new Label();
                            CharInputLabel.Content = Label;
                            var CharInput = new TextBox();
                            CharInput.Width = 60;
                            CharInput.PreviewTextInput += (sender, e) => { e.Handled = CharInput.Text != ""; };
                            CharInputContainer.Children.Add(CharInputLabel);
                            CharInputContainer.Children.Add(CharInput);
                            SectionContent.Children.Add(CharInputContainer);
                            break;
                        case "Checkbox":
                            var cbCustomisation = new CheckBox();
                            cbCustomisation.Content = Label;
                            cbCustomisation.Margin = new Thickness(10, lastTop + 10, 0, 0);
                            lastMargin = cbCustomisation.Margin;
                            cbCustomisation.IsChecked = ControlItem.Default == "1";
                            SectionContent.Children.Add(cbCustomisation);
                            break;
                        case "Color":
                        case "Colour":
                        case "ColorPicker":
                        case "ColourPicker":
                            var ColourInputContainer = new StackPanel();
                            ColourInputContainer.Margin = new Thickness(10, lastTop, 0, 10);
                            var ColourInputLabel = new Label();
                            ColourInputLabel.Content = Label;
                            ColourInputLabel.FontSize = 16;
                            var ColourInput = new ColorPicker();
                            try
                            {
                                ColourInput.SelectedColor =
                                    (Color) new ColorConverter().ConvertFrom(ControlItem.Default);
                            }
                            catch
                            {
                                ColourInput.SelectedColor = Color.FromArgb(255, 0, 255, 0);
                            }

                            ColourInputContainer.Children.Add(ColourInputLabel);
                            ColourInputContainer.Children.Add(ColourInput);
                            SectionContent.Children.Add(ColourInputContainer);
                            break;
                        case "DropDown":
                        case "DropDownMenu":
                        case "Select":
                        case "ComboBox":
                            var ComboBoxContainer = new StackPanel();
                            ComboBoxContainer.Margin = new Thickness(10, lastTop, 0, 10);
                            var ComboBoxLabel = new Label();
                            ComboBoxLabel.Content = Label;
                            ComboBoxLabel.FontSize = 16;
                            var ComboBoxCustomisation = new ComboBox();
                            if (ControlItem.Options == null) break;
                            if (ControlItem.Options.Length <= 0) break;
                            foreach (var option in ControlItem.Options)
                            {
                                var OptionLabel = option.Label;
                                var OptionValue = option.Value;
                                ComboBoxCustomisation.Items.Add(OptionLabel);
                            }

                            ComboBoxCustomisation.SelectedIndex =
                                int.TryParse(ControlItem.Default, out var index) ? index : 1;
                            ComboBoxContainer.Children.Add(ComboBoxLabel);
                            ComboBoxContainer.Children.Add(ComboBoxCustomisation);

                            ComboBoxContainer.Margin = new Thickness(10, lastTop, 0, 10);
                            SectionContent.Children.Add(ComboBoxContainer);
                            break;
                        case "Number":
                            var NumberInputContainer = new WrapPanel();
                            NumberInputContainer.Margin = new Thickness(10, lastTop, 0, 0);
                            var NumberInputLabel = new Label();
                            NumberInputLabel.Content = Label;
                            var NumberInput = new TextBox();
                            NumberInput.Width = 60;
                            NumberInput.PreviewTextInput += (sender, e) =>
                            {
                                e.Handled = !Regex.IsMatch(e.Text, "\\d");
                            };
                            NumberInputContainer.Children.Add(NumberInputLabel);
                            NumberInputContainer.Children.Add(NumberInput);
                            SectionContent.Children.Add(NumberInputContainer);
                            break;
                        case "IntegerUpDown":
                            var IntegerInputContainer = new StackPanel();
                            IntegerInputContainer.Margin = new Thickness(10, lastTop, 0, 10);
                            var IntegerInputLabel = new Label();
                            IntegerInputLabel.Content = Label;
                            IntegerInputLabel.FontSize = 16;
                            var IntegerInput = new IntegerUpDown();
                            IntegerInput.Value = int.TryParse(ControlItem.Default, out index) ? index : 0;
                            IntegerInput.Minimum = ControlItem.Minimum;
                            IntegerInput.Maximum = ControlItem.Maximum;
                            IntegerInput.Increment = ControlItem.Increment;
                            IntegerInputContainer.Children.Add(IntegerInputLabel);
                            IntegerInputContainer.Children.Add(IntegerInput);
                            SectionContent.Children.Add(IntegerInputContainer);
                            break;
                        default:
                            throw new Exception($"Type {Type} is not a valid type!");
                    }

                    // lastTop = lastMargin.Top + 10;
                }

                SectionContainer.Content = SectionContent;

                if (Layout != null)
                {
                    // Avoid evaluating unnecessarily
                    bool GroupBoxItemEvaluated = false;

                    for (int i = 0; i < Layout.Length; i++)
                    {
                        for (int j = 0; j < Layout[i].Length; j++)
                        {
                            // Allow index and grid area for grid coordinates
                            if (GroupBoxIndex.ToString() == Layout[i][j] || Section == Layout[i][j] && !GroupBoxItemEvaluated)
                            {
                                // Dont set column or row if it has already been set
                                // setting the column/row every time will break spans
                                if (Grid.GetColumn(SectionContainer) == 0)
                                {
                                    Grid.SetColumn(SectionContainer, j);
                                }
                                if (Grid.GetRow(SectionContainer) == 0)
                                {
                                    Grid.SetRow(SectionContainer, i);
                                }

                                // These are not optimal speed but the code should be easier to understand:
                                // Counts the occurences of the current item id/index
                                int ColumnSpan = 0;
                                // Iterate current row 
                                for (int TempColumnIndex = 0; TempColumnIndex < Layout[i].Length; TempColumnIndex++)
                                {
                                    if (GroupBoxIndex.ToString() == Layout[i][TempColumnIndex] || Section == Layout[i][TempColumnIndex])
                                    {
                                        ColumnSpan++;
                                    }
                                }
                                Grid.SetColumnSpan(SectionContainer, ColumnSpan);

                                int RowSpan = 0;
                                for (int TempRowIndex = 0; TempRowIndex < Layout.Length; TempRowIndex++)
                                {
                                    if (GroupBoxIndex.ToString() == Layout[TempRowIndex][j] || Section == Layout[TempRowIndex][j])
                                    {
                                        RowSpan++;
                                    }
                                }
                                Grid.SetRowSpan(SectionContainer, RowSpan);

                                // Break parent loop
                                GroupBoxItemEvaluated = true;
                                break;
                            }

                            if (GroupBoxItemEvaluated)
                            {
                                break;
                            }
                        }
                    }
                }

                SectionsContainer.Children.Add(SectionContainer);
                GroupBoxIndex++;
            }

            Container.Children.Add(SectionsContainer);
            Controls.Children.Add(Container);

            ControlsRendered = true;
            return Controls;
        }

        public void Update()
        {
            if (UpdateUrl != null) MainWindow.DownloadHud(UpdateUrl);
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

        [JsonPropertyName("Type")] public string Type;
    }

    public class Option
    {
        [JsonPropertyName("Label")] public string Label;

        [JsonPropertyName("Value")] public string Value;
    }

    #endregion
}