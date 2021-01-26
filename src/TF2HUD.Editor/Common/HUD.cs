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
        public Controls ControlOptions;
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

            var lastMargin = new Thickness(10, 10, 0, 0);
            var lastTop = lastMargin.Top;
            var SectionContainer = new GroupBox();
            SectionContainer.Header = "New Section"; // TODO: Use section name from JSON
            SectionContainer.Margin = lastMargin;
            SectionContainer.Width = 330;
            var SectionContent = new WrapPanel();

            // Render Controls Here
            foreach (var control in ControlOptions.General)
            {
                var Label = control.Label;
                var Type = control.Type;
                var File = control.File;

                switch (Type)
                {
                    case "Checkbox":
                        var cbCustomisation = new CheckBox();
                        cbCustomisation.Content = Label;
                        cbCustomisation.Margin = new Thickness(10, lastTop, 0, 0);
                        lastMargin = cbCustomisation.Margin;
                        SectionContent.Children.Add(cbCustomisation);
                        break;
                    case "DropDown":
                    case "DropDownMenu":
                    case "Select":
                    case "ComboBox":
                        var ComboBoxCustomisation = new ComboBox();
                        if (control.Options.Count <= 0) break;
                        foreach (var option in control.Options)
                        {
                            var OptionLabel = option.Label;
                            var OptionValue = option.Value;
                            ComboBoxCustomisation.Items.Add(OptionLabel);
                        }

                        ComboBoxCustomisation.Margin = new Thickness(10, lastTop, 0, 10);
                        SectionContent.Children.Add(ComboBoxCustomisation);

                        break;
                    default:
                        throw new Exception($"Type {Type} is not a valid type!");
                }

                lastTop = lastMargin.Top + 10;
            }

            SectionContainer.Content = SectionContent;
            Controls.Children.Add(SectionContainer);

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
        [JsonPropertyName("Controls")] public Controls Controls;

        [JsonPropertyName("CustomisationsFolder")]
        public string CustomisationsFolder;

        [JsonPropertyName("EnabledFolder")] public string EnabledFolder;

        [JsonPropertyName("UpdateUrl")] public string UpdateUrl;
    }

    public class Controls
    {
        [JsonPropertyName("General")] public List<General> General;
    }

    public class General
    {
        [JsonPropertyName("File")] public string File;

        [JsonPropertyName("Label")] public string Label;

        [JsonPropertyName("Options")] public List<Option> Options;

        [JsonPropertyName("Type")] public string Type;
    }

    public class Option
    {
        [JsonPropertyName("Label")] public string Label;

        [JsonPropertyName("Value")] public string Value;
    }

    #endregion
}