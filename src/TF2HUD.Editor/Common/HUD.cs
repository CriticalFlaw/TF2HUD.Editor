using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace TF2HUD.Editor.Common
{
    public class HUD
    {
        private readonly Grid Controls = new();
        public Dictionary<string, Control[]> ControlOptions;
        private bool ControlsRendered;
        public string CustomisationsFolder;
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
        }

        public Grid GetControls()
        {
            if (ControlsRendered) return Controls;

            Controls.RowDefinitions.Add(new RowDefinition());
            Controls.RowDefinitions.Add(new RowDefinition());
            Controls.Margin = new Thickness(10, 10, 10, 10);


            var CustomiserTitle = new Label();
            CustomiserTitle.Content = this.Name;
            CustomiserTitle.FontSize = 35;
            Grid.SetRow(CustomiserTitle, 0);
            Controls.Children.Add(CustomiserTitle);

            //var lastMargin = new Thickness(10, 10, 0, 0);
            // var lastTop = lastMargin.Top;
            // SectionContainer.Margin = lastMargin;
            // SectionContainer.Width = 330;

            var SectionsContainer = new WrapPanel();
            Grid.SetRow(SectionsContainer, 1);

            foreach (var Section in ControlOptions.Keys)
            {
                var SectionContainer = new GroupBox();
                SectionContainer.Header = Section;
                SectionContainer.Margin = new Thickness(10);

                var SectionContent = new StackPanel();
                SectionContent.Margin = new Thickness(15, 5, 15, 5);

                foreach (var ControlItem in ControlOptions[Section])
                {
                    var Label = ControlItem.Label;
                    var Type = ControlItem.Type;
                    // var File = ControlItem.File;

                    switch (Type)
                    {
                        case "Checkbox":
                            var cbCustomisation = new CheckBox();
                            cbCustomisation.Content = Label;
                            // cbCustomisation.Margin = new Thickness(10, lastTop, 0, 0);
                            // lastMargin = cbCustomisation.Margin;
                            SectionContent.Children.Add(cbCustomisation);
                            break;
                        case "Color":
                        case "Colour":
                            var ColourInputContainer = new StackPanel();
                            var ColoutInputLabel = new Label();
                            ColoutInputLabel.Content = Label;
                            var ColourInput = new Xceed.Wpf.Toolkit.ColorPicker();
                            ColourInput.SelectedColor = System.Windows.Media.Color.FromArgb(255, 0, 255, 0);
                            ColourInputContainer.Children.Add(ColoutInputLabel);
                            ColourInputContainer.Children.Add(ColourInput);
                            SectionContent.Children.Add(ColourInputContainer);
                            break;
                        case "DropDown":
                        case "DropDownMenu":
                        case "Select":
                        case "ComboBox":
                            var ComboBoxContainer = new WrapPanel();
                            var ComboBoxLabel = new Label();
                            ComboBoxLabel.Content = Label;
                            var ComboBoxCustomisation = new ComboBox();
                            if (ControlItem.Options == null) break;
                            if (ControlItem.Options.Length <= 0) break;
                            foreach (var option in ControlItem.Options)
                            {
                                var OptionLabel = option.Label;
                                var OptionValue = option.Value;
                                ComboBoxCustomisation.Items.Add(OptionLabel);
                            }
                            ComboBoxContainer.Children.Add(ComboBoxLabel);
                            ComboBoxContainer.Children.Add(ComboBoxCustomisation);

                            // ComboBoxCustomisation.Margin = new Thickness(10, lastTop, 0, 10);
                            SectionContent.Children.Add(ComboBoxContainer);

                            break;
                        default:
                            throw new Exception($"Type {Type} is not a valid type!");
                    }

                    // lastTop = lastMargin.Top + 10;
                }
                SectionContainer.Content = SectionContent;
                SectionsContainer.Children.Add(SectionContainer);
            }
            Controls.Children.Add(SectionsContainer);

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

        [JsonPropertyName("UpdateUrl")] public string UpdateUrl;
    }

    public class Control
    {
        [JsonPropertyName("File")] public string Label;

        [JsonPropertyName("File")] public string Type;

        [JsonPropertyName("File")] public Option[] Options;

    }

	public class Option
	{
        [JsonPropertyName("File")] public string Label;

        [JsonPropertyName("File")] public string Value;
    }


    #endregion
}