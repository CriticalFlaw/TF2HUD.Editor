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
    public class HUD
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

        #region PAGE UI

        /// <summary>
        ///     Generate the page layout using controls defined in the HUD schema.
        /// </summary>
        public Grid GetControls()
        {
            // Skip this process if the controls have already been rendered.
            if (isRendered) return controls;

            // Define the container that will hold the title and content.
            var container = new Grid();
            var titleRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            var contentRow = new RowDefinition();
            if (layout != null) contentRow.Height = GridLength.Auto;
            container.RowDefinitions.Add(titleRow);
            container.RowDefinitions.Add(contentRow);

            // Define the title of the HUD displayed at the top of the page.
            var title = new Label
            {
                Style = (Style) Application.Current.Resources["PageTitle"],
                Content = Name
            };
            Grid.SetRow(title, 0);
            container.Children.Add(title);

            // Create the preview modal
            var preview = new ChildWindow
            {
                Style = (Style) Application.Current.Resources["PreviewPanel"]
            };
            preview.MouseDoubleClick += (_, _) => { preview.Close(); };

            var image = new Image
            {
                Style = (Style) Application.Current.Resources["PreviewImage"]
            };
            preview.Content = image;

            container.Children.Add(preview);

            // NOTE: ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic for each section.
            dynamic sectionsContainer;

            if (LayoutOptions != null)
            {
                // Splits Layout string[] into 2D Array using \s+
                layout = LayoutOptions.Select(t => Regex.Split(t, "\\s+")).ToArray();

                sectionsContainer = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    MaxWidth = 1270,
                    MaxHeight = 720
                };

                // Assume that all row arrays are the same length, use column information from Layout[0].
                for (var i = 0; i < layout[0].Length; i++)
                    sectionsContainer.ColumnDefinitions.Add(new ColumnDefinition());
                for (var i = 0; i < layout.Length; i++)
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

            // Generate each control section as defined in the schema.
            foreach (var section in ControlOptions.Keys)
            {
                var sectionContainer = new GroupBox
                {
                    Header = section
                };

                Panel sectionContent = layout != null ? new WrapPanel() : new StackPanel();
                sectionContent.Margin = new Thickness(3);

                // Generate each individual control, add it to user settings.
                foreach (var controlItem in ControlOptions[section])
                {
                    var id = controlItem.Name;
                    var label = controlItem.Label;
                    var tooltip = controlItem.Tooltip;
                    Settings.AddSetting(id, controlItem);

                    switch (controlItem.Type.ToLowerInvariant())
                    {
                        case "checkbox":
                            // Create the Control.
                            var checkBoxInput = new CheckBox
                            {
                                Name = id,
                                Content = label,
                                Margin = new Thickness(10, lastTop + 10, 30, 0),
                                IsChecked = Settings.GetSetting<bool>(id),
                                ToolTip = tooltip
                            };

                            // Add Events.
                            checkBoxInput.Checked += (sender, _) =>
                            {
                                var input = sender as CheckBox;
                                Settings.SetSetting(input?.Name, "true");
                                CheckIsDirty(controlItem);
                            };
                            checkBoxInput.Unchecked += (sender, _) =>
                            {
                                var input = sender as CheckBox;
                                Settings.SetSetting(input?.Name, "false");
                                CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            sectionContent.Children.Add(checkBoxInput);
                            controlItem.Control = checkBoxInput;

                            // Create a preview button if the control has a preview image.
                            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                            {
                                var previewBtn = new Button
                                {
                                    Style = (Style) Application.Current.Resources["PreviewButton"]
                                };
                                previewBtn.Click += (_, _) =>
                                {
                                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip) ? tooltip : id;
                                    image.Source = new BitmapImage(new Uri(controlItem.Preview));
                                    preview.Show();
                                };
                                sectionContent.Children.Add(previewBtn);
                            }

                            break;

                        case "color":
                        case "colour":
                        case "colorpicker":
                        case "colourpicker":
                            // Create the Control.
                            var colorContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var colorLabel = new Label
                            {
                                Content = label,
                                Style = (Style) Application.Current.Resources["ColorPickerLabel"]
                            };
                            var colorInput = new ColorPicker
                            {
                                Name = id,
                                ToolTip = tooltip
                            };

                            // Attempt to bind the color from the settings.
                            try
                            {
                                colorInput.SelectedColor = Settings.GetSetting<Color>(id);
                            }
                            catch
                            {
                                colorInput.SelectedColor = Color.FromArgb(255, 0, 255, 0);
                            }

                            // Add Events.
                            colorInput.SelectedColorChanged += (sender, _) =>
                            {
                                var input = sender as ColorPicker;
                                Settings.SetSetting(input?.Name,
                                    Utilities.ConvertToRgba(input?.SelectedColor.ToString()));
                            };
                            colorInput.Closed += (sender, _) =>
                            {
                                var input = sender as ColorPicker;
                                Settings.SetSetting(input?.Name,
                                    Utilities.ConvertToRgba(input?.SelectedColor.ToString()));
                                CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            colorContainer.Children.Add(colorLabel);
                            colorContainer.Children.Add(colorInput);
                            sectionContent.Children.Add(colorContainer);
                            controlItem.Control = colorInput;

                            // Create a preview button if the control has a preview image.
                            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                            {
                                var previewBtn = new Button
                                {
                                    Style = (Style) Application.Current.Resources["PreviewButton"]
                                };
                                previewBtn.Click += (_, _) =>
                                {
                                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                        ? tooltip
                                        : id;
                                    image.Source = new BitmapImage(new Uri(controlItem.Preview));
                                    preview.Show();
                                };
                                sectionContent.Children.Add(previewBtn);
                            }

                            break;

                        case "dropdown":
                        case "dropdownmenu":
                        case "select":
                        case "combobox":
                            // Do not create a ComboBox if there are no defined options.
                            if (controlItem.Options is not {Length: > 0}) break;

                            // Create the Control.
                            var comboBoxContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var comboBoxLabel = new Label
                            {
                                Content = label,
                                Style = (Style) Application.Current.Resources["ComboBoxLabel"]
                            };
                            var comboBoxInput = new ComboBox
                            {
                                Name = id,
                                ToolTip = tooltip
                            };

                            // Add items to the ComboBox.
                            foreach (var option in controlItem.Options)
                            {
                                var item = new ComboBoxItem
                                {
                                    Content = option.Label
                                };

                                comboBoxInput.Items.Add(item);
                            }

                            // Set the selected value depending on the what's retrieved from the setting file.
                            var comboValue = Settings.GetSetting<string>(id);
                            if (!Regex.IsMatch(comboValue, "\\D"))
                                comboBoxInput.SelectedIndex = int.Parse(comboValue);
                            else
                                comboBoxInput.SelectedValue = comboValue;

                            // Add Events.
                            comboBoxInput.SelectionChanged += (sender, _) =>
                            {
                                var input = sender as ComboBox;
                                Settings.SetSetting(input?.Name, comboBoxInput.SelectedIndex.ToString());
                                CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            comboBoxContainer.Children.Add(comboBoxLabel);
                            comboBoxContainer.Children.Add(comboBoxInput);
                            sectionContent.Children.Add(comboBoxContainer);
                            controlItem.Control = comboBoxInput;

                            // Create a preview button if the control has a preview image.
                            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                            {
                                var previewBtn = new Button
                                {
                                    Style = (Style) Application.Current.Resources["PreviewButton"]
                                };
                                previewBtn.Click += (_, _) =>
                                {
                                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                        ? tooltip
                                        : id;
                                    image.Source = new BitmapImage(new Uri(controlItem.Preview));
                                    preview.Show();
                                };
                                sectionContent.Children.Add(previewBtn);
                            }

                            break;

                        case "number":
                        case "integer":
                        case "integerupdown":
                            // Create the Control.
                            var integerContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var integerLabel = new Label
                            {
                                Content = label,
                                Style = (Style) Application.Current.Resources["IntegerUpDownLabel"]
                            };
                            var integerInput = new IntegerUpDown
                            {
                                Name = id,
                                Value = Settings.GetSetting<int>(id),
                                Minimum = controlItem.Minimum,
                                Maximum = controlItem.Maximum,
                                Increment = controlItem.Increment,
                                ToolTip = tooltip
                            };

                            // Add Events.
                            integerInput.ValueChanged += (sender, _) =>
                            {
                                var input = sender as IntegerUpDown;
                                Settings.SetSetting(input?.Name, input.Text);
                                CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            integerContainer.Children.Add(integerLabel);
                            integerContainer.Children.Add(integerInput);
                            sectionContent.Children.Add(integerContainer);
                            controlItem.Control = integerInput;

                            // Create a preview button if the control has a preview image.
                            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                            {
                                var previewBtn = new Button
                                {
                                    Style = (Style) Application.Current.Resources["PreviewButton"]
                                };
                                previewBtn.Click += (_, _) =>
                                {
                                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                        ? tooltip
                                        : id;
                                    image.Source = new BitmapImage(new Uri(controlItem.Preview));
                                    preview.Show();
                                };
                                sectionContent.Children.Add(previewBtn);
                            }

                            break;

                        case "crosshair":
                        case "customcrosshair":
                            // Create the Control.
                            var xhairContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var xhairLabel = new Label
                            {
                                Content = label,
                                Style = (Style) Application.Current.Resources["CrosshairLabel"]
                            };
                            var xhairInput = new ComboBox
                            {
                                Name = id,
                                ToolTip = tooltip
                            };

                            // Add items to the ComboBox.
                            foreach (var item in Utilities.CrosshairStyles.Select(option => new ComboBoxItem
                            {
                                Content = option,
                                Style = (Style) Application.Current.Resources["Crosshair"]
                            }))
                            {
                                xhairInput.Style = (Style) Application.Current.Resources["CrosshairBox"];
                                xhairInput.Items.Add(item);
                            }

                            // Set the selected value depending on the what's retrieved from the setting file.
                            var xhairValue = Settings.GetSetting<string>(id);
                            if (!Regex.IsMatch(xhairValue, "\\D"))
                                xhairInput.SelectedIndex = int.Parse(xhairValue);
                            else
                                xhairInput.SelectedValue = xhairValue;

                            // Add Events.
                            xhairInput.SelectionChanged += (sender, _) =>
                            {
                                var input = sender as ComboBox;
                                Settings.SetSetting(input?.Name, xhairInput.SelectedValue.ToString());
                            };

                            // Add to Page.
                            xhairContainer.Children.Add(xhairLabel);
                            xhairContainer.Children.Add(xhairInput);
                            sectionContent.Children.Add(xhairContainer);
                            controlItem.Control = xhairInput;

                            // Create a preview button if the control has a preview image.
                            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                            {
                                var previewBtn = new Button
                                {
                                    Style = (Style) Application.Current.Resources["PreviewButton"]
                                };
                                previewBtn.Click += (_, _) =>
                                {
                                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                        ? tooltip
                                        : id;
                                    image.Source = new BitmapImage(new Uri(controlItem.Preview));
                                    preview.Show();
                                };
                                sectionContent.Children.Add(previewBtn);
                            }

                            break;

                        case "custombackground":
                            // Create the Control.
                            var bgInput = new Button
                            {
                                Name = id,
                                Content = label,
                                Height = 32,
                                Margin = new Thickness(10, lastTop + 10, 0, 0),
                                Padding = new Thickness(5, 2, 5, 0),
                                ToolTip = tooltip
                            };

                            // Add Events.
                            bgInput.Click += (_, _) =>
                            {
                                //if (MainWindow.ShowMessageBox(MessageBoxImage.Warning, Resources.ask_custom_background, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                                //var imagePath = new BackgroundManager($"{MainWindow.HudPath}\\{Name}\\").ApplyCustomBackground();
                                //Settings.SetSetting(bgInput.Name, imagePath);
                                //CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            sectionContent.Children.Add(bgInput);
                            controlItem.Control = bgInput;
                            break;

                        case "homeserver":
                            // Create the Control.
                            var serverContainer = new StackPanel
                            {
                                Margin = new Thickness(10, lastTop, 0, 10)
                            };
                            var serverLabel = new Label
                            {
                                Content = label,
                                FontSize = 18
                            };
                            var serverInput = new TextBox()
                            {
                                Name = id,
                                Text = controlItem.Value,
                                ToolTip = tooltip
                            };

                            // Add Events.
                            serverInput.LostFocus += (_, _) =>
                            {
                                Settings.SetSetting(serverInput.Name, serverInput.Text);
                                CheckIsDirty(controlItem);
                            };

                            // Add to Page.
                            serverContainer.Children.Add(serverLabel);
                            serverContainer.Children.Add(serverInput);
                            sectionContent.Children.Add(serverContainer);
                            controlItem.Control = serverInput;
                            break;

                        default:
                            throw new Exception($"Entered type {controlItem.Type} is invalid.");
                    }
                }

                sectionContainer.Content = sectionContent;

                if (layout != null)
                {
                    // Avoid evaluating unnecessarily
                    var groupBoxItemEvaluated = false;

                    for (var i = 0; i < layout.Length; i++)
                    for (var j = 0; j < layout[i].Length; j++)
                    {
                        // Allow index and grid area for grid coordinates.
                        if (groupBoxIndex.ToString() == layout[i][j] && !groupBoxItemEvaluated)
                        {
                            // Don't set column or row if it has already been set.
                            // Setting the column/row every time will break spans.
                            if (Grid.GetColumn(sectionContainer) == 0) Grid.SetColumn(sectionContainer, j);
                            if (Grid.GetRow(sectionContainer) == 0) Grid.SetRow(sectionContainer, i);

                            // These are not optimal speed but the code should be easier to understand:
                            // Counts the occurrences of the current item id/index
                            var columnSpan = 0;
                            // Iterate current row
                            for (var index = 0; index < layout[i].Length; index++)
                                if (groupBoxIndex.ToString() == layout[i][index] || section == layout[i][index])
                                    columnSpan++;
                            Grid.SetColumnSpan(sectionContainer, columnSpan);

                            var rowSpan = 0;
                            for (var TempRowIndex = 0; TempRowIndex < layout.Length; TempRowIndex++)
                                if (groupBoxIndex.ToString() == layout[TempRowIndex][j] ||
                                    section == layout[TempRowIndex][j])
                                    rowSpan++;
                            Grid.SetRowSpan(sectionContainer, rowSpan);

                            // Break parent loop
                            groupBoxItemEvaluated = true;
                            break;
                        }

                        if (groupBoxItemEvaluated) break;
                    }
                }

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = sectionContainer.Content,
                    Background = new SolidColorBrush(Colors.Transparent)
                };
                sectionContainer.Content = scrollViewer;
                sectionsContainer.Children.Add(sectionContainer);
                groupBoxIndex++;
            }

            container.Children.Add(sectionsContainer);
            controls.Children.Add(container);

            isRendered = true;
            return controls;
        }

        #endregion

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

            void LogChange(string prop, string before = "", string after= "")
            {
                string message = before.Length > 0 ? $" (\"{before}\" => \"{after}\")" : string.Empty;
                MainWindow.Logger.Info($"{Name}: {prop} has changed{message}, HUD has been updated.");
            }

            bool CompareFiles(JObject obj1, JObject obj2, string path = "")
            {
                foreach (var x in obj1)
                {
                    if (!obj2.ContainsKey(x.Key))
                    {
                        return false;
                    }
                    else if (obj1[x.Key].Type == JTokenType.Object && obj2[x.Key].Type == JTokenType.Object)
                    {
                        if (!CompareFiles(obj1[x.Key].ToObject<JObject>(), obj2[x.Key].ToObject<JObject>(), x.Key + "/"))
                        {
                            return false;
                        }
                    }
                    else if (x.Value.Type == JTokenType.Array && obj2[x.Key].Type == JTokenType.Array)
                    {
                        var arr1 = obj1[x.Key].ToArray();
                        var arr2 = obj2[x.Key].ToArray();
                        if (arr1.Length != arr2.Length)
                        {
                            LogChange($"{path}{x.Key}", arr1.Length.ToString(), arr2.Length.ToString());
                            return false;
                        }
                        for (var i = 0; i < arr1.Length; i++)
                        {
                            if (arr1[i].ToString() != arr2[i].ToString())
                            {
                                LogChange($"{path}{x.Key}/[{i}]", arr1[i].ToString(), arr2[i].ToString());
                                return false;
                            }
                        }
                    }
                    else if (x.Value != x.Value)
                    {
                        return false;
                    }
                }
                return true;
            }

            if (LayoutOptions != null && hud.LayoutOptions != null)
            {
                if (LayoutOptions.Length != hud.LayoutOptions.Length)
                {
                    LogChange("LayoutOptions.Length");
                    return false;
                }
                for (var i = 0; i < LayoutOptions.Length; i++)
                {
                    if (LayoutOptions[i] != hud.LayoutOptions[i])
                    {
                        LogChange($"LayoutOptions[{i}]", LayoutOptions[i], hud.LayoutOptions[i]);
                        return false;
                    }
                }
            }

            if (Background != hud.Background)
            {
                LogChange("Background", Background, hud.Background);
                return false;
            };

            if (ControlOptions.Keys.Count != hud.ControlOptions.Keys.Count)
            {
                LogChange("ControlOptions.Keys.Count", ControlOptions.Keys.Count.ToString(), hud.ControlOptions.Keys.Count.ToString());
                return false;
            }
            var keys1 = ControlOptions.Keys.ToArray();
            var keys2 = hud.ControlOptions.Keys.ToArray();
            for (var i = 0; i < keys1.Length; i++)
            {
                if (keys1[i] != keys2[i])
                {
                    LogChange($"GroupBox {keys1[i]}", keys1[i], keys2[i]);
                    return false;
                }
            }

            foreach (var key in ControlOptions.Keys)
            {
                if (ControlOptions[key].Length != hud.ControlOptions[key].Length)
                {
                    LogChange($"ControlOptions[{key}].Length", ControlOptions[key].Length.ToString(), hud.ControlOptions[key].Length.ToString());
                    return false;
                }
                for (var i = 0; i < ControlOptions[key].Length; i++)
                {
                    if (ControlOptions[key][i].ComboFiles != hud.ControlOptions[key][i].ComboFiles)
                    {
                        if (ControlOptions[key][i].ComboFiles.Length != hud.ControlOptions[key][i].ComboFiles.Length)
                        {
                            LogChange($"ControlOptions[\"{key}\"][{i}].ComboFiles.Length", ControlOptions[key][i].ComboFiles.Length.ToString(), hud.ControlOptions[key][i].ComboFiles.Length.ToString());
                            return false;
                        }
                        for (var j = 0; j < ControlOptions[key][i].ComboFiles.Length; j++)
                        {
                            if (ControlOptions[key][i].ComboFiles[j] != hud.ControlOptions[key][i].ComboFiles[j])
                            {
                                LogChange($"ControlOptions[\"{key}\"][{i}].ComboFiles[{j}]", ControlOptions[key][i].ComboFiles[j], hud.ControlOptions[key][i].ComboFiles[j]);
                                return false;
                            }
                        }
                    }
                    if (ControlOptions[key][i].FileName != hud.ControlOptions[key][i].FileName)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].FileName", ControlOptions[key][i].FileName, hud.ControlOptions[key][i].FileName);
                        return false;
                    }
                    if (ControlOptions[key][i].Files != null && hud.ControlOptions[key][i].Files != null)
                    {
                        if (!CompareFiles(ControlOptions[key][i].Files, hud.ControlOptions[key][i].Files))
                        {
                            LogChange($"ControlOptions[{key}][{i}].Files");
                            return false;
                        }
                    }
                    if (ControlOptions[key][i].Increment != hud.ControlOptions[key][i].Increment)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Increment", ControlOptions[key][i].Increment.ToString(), hud.ControlOptions[key][i].Increment.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Label != hud.ControlOptions[key][i].Label)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Label", ControlOptions[key][i].Label, hud.ControlOptions[key][i].Label);
                        return false;
                    }
                    if (ControlOptions[key][i].Maximum != hud.ControlOptions[key][i].Maximum)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Maximum", ControlOptions[key][i].Maximum.ToString(), hud.ControlOptions[key][i].Maximum.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Minimum != hud.ControlOptions[key][i].Minimum)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Minimum", ControlOptions[key][i].Minimum.ToString(), hud.ControlOptions[key][i].Minimum.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Name != hud.ControlOptions[key][i].Name)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Name", ControlOptions[key][i].Name, hud.ControlOptions[key][i].Name);
                        return false;
                    }
                    if (ControlOptions[key][i].Options != hud.ControlOptions[key][i].Options)
                    {
                        if (ControlOptions[key][i].Options.Length != hud.ControlOptions[key][i].Options.Length)
                        {
                            LogChange($"ControlOptions[\"{key}\"][{i}].Options.Length", ControlOptions[key][i].Options.Length.ToString(), hud.ControlOptions[key][i].Options.Length.ToString());
                            return false;
                        }
                        for (var j = 0; j < ControlOptions[key][i].Options.Length; j++)
                        {
                            if (ControlOptions[key][i].Options[j].FileName != hud.ControlOptions[key][i].Options[j].FileName)
                            {
                                LogChange($"ControlOptions[{key}][{i}].Options[{j}].FileName", ControlOptions[key][i].Options[j].FileName, hud.ControlOptions[key][i].Options[j].FileName);
                                return false;
                            }
                            if (ControlOptions[key][i].Options[j].Label != hud.ControlOptions[key][i].Options[j].Label)
                            {
                                LogChange($"ControlOptions[{key}][{i}].Options[{j}].Label", ControlOptions[key][i].Options[j].Label, hud.ControlOptions[key][i].Options[j].Label);
                                return false;
                            }
                            if (ControlOptions[key][i].Options[j].Special != hud.ControlOptions[key][i].Options[j].Special)
                            {
                                LogChange($"ControlOptions[{key}][{i}].Options[{j}].Special", ControlOptions[key][i].Options[j].Special, hud.ControlOptions[key][i].Options[j].Special);
                                return false;
                            }
                            if (ControlOptions[key][i].Options[j].Value != hud.ControlOptions[key][i].Options[j].Value)
                            {
                                LogChange($"ControlOptions[{key}][{i}].Options[{j}].Value", ControlOptions[key][i].Options[j].Value, hud.ControlOptions[key][i].Options[j].Value);
                                return false;
                            }
                        }
                    }
                    if (ControlOptions[key][i].Preview != hud.ControlOptions[key][i].Preview)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Preview", ControlOptions[key][i].Preview.ToString(), hud.ControlOptions[key][i].Preview.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Pulse != hud.ControlOptions[key][i].Pulse)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Pulse", ControlOptions[key][i].Pulse.ToString(), hud.ControlOptions[key][i].Pulse.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Restart != hud.ControlOptions[key][i].Restart)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Restart", ControlOptions[key][i].Restart.ToString(), hud.ControlOptions[key][i].Restart.ToString());
                        return false;
                    }
                    if (ControlOptions[key][i].Special != hud.ControlOptions[key][i].Special)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Special", ControlOptions[key][i].Special, hud.ControlOptions[key][i].Special);
                        return false;
                    }
                    if (ControlOptions[key][i].Tooltip != hud.ControlOptions[key][i].Tooltip)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Tooltip", ControlOptions[key][i].Tooltip, hud.ControlOptions[key][i].Tooltip);
                        return false;
                    }
                    if (ControlOptions[key][i].Type != hud.ControlOptions[key][i].Type)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Type", ControlOptions[key][i].Type, hud.ControlOptions[key][i].Type);
                        return false;
                    }
                    if (ControlOptions[key][i].Value != hud.ControlOptions[key][i].Value)
                    {
                        LogChange($"ControlOptions[\"{key}\"][{i}].Value", ControlOptions[key][i].Value, hud.ControlOptions[key][i].Value);
                        return false;
                    }
                }
            }

            if (CustomizationsFolder != hud.CustomizationsFolder)
            {
                LogChange("CustomizationsFolder", CustomizationsFolder, hud.CustomizationsFolder);
                return false;
            }

            if (EnabledFolder != hud.EnabledFolder)
            {
                LogChange("EnabledFolder", EnabledFolder, hud.EnabledFolder);
                return false;
            }

            if (Maximize != hud.Maximize)
            {
                LogChange("Maximize", Maximize.ToString(), hud.Maximize.ToString());
                return false;
            }

            if (Opacity != hud.Opacity)
            {
                LogChange("Opacity", Opacity.ToString(), hud.Opacity.ToString());
                return false;
            };
            if (UpdateUrl != hud.UpdateUrl)
            {
                LogChange("UpdateUrl", UpdateUrl, hud.UpdateUrl);
                return false;
            }

            if (GitHubUrl != hud.GitHubUrl)
            {
                LogChange("GitHubUrl", GitHubUrl, hud.GitHubUrl);
                return false;
            }

            if (IssueUrl != hud.IssueUrl)
            {
                LogChange("IssueUrl", IssueUrl, hud.IssueUrl);
                return false;
            }

            if (HudsTfUrl != hud.HudsTfUrl)
            {
                LogChange("HudsTfUrl", HudsTfUrl, hud.HudsTfUrl);
                return false;
            }

            if (SteamUrl != hud.SteamUrl)
            {
                LogChange("SteamUrl", SteamUrl, hud.SteamUrl);
                return false;
            }

            if (DiscordUrl != hud.DiscordUrl)
            {
                LogChange("DiscordUrl", DiscordUrl, hud.DiscordUrl);
                return false;
            }

            return true;
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
                                if (((ComboBoxItem) combo.Items[0]).Style ==
                                    (Style) Application.Current.Resources["Crosshair"])
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

        /// <summary>
        ///     Apply user selected customizations to the HUD files.
        /// </summary>
        public bool ApplyCustomizations()
        {
            try
            {
                // HUD Background Image.
                // Set the HUD Background image path when applying, because it's possible
                // the user did not have their tf/custom folder set up when this HUD
                // constructor was called
                HUDBackground = new BackgroundManager($"{MainWindow.HudPath}\\{Name}\\");

                var hudSettings = ControlOptions.Values;
                // var hudSettings = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json"))
                //     .Controls.Values;

                // If the developer defined customization folders for their HUD, then copy those files.
                if (!string.IsNullOrWhiteSpace(CustomizationsFolder))
                    MoveCustomizationFiles(hudSettings);

                // This Dictionary contains folders/files/properties as they should be written to the hud
                // the 'IterateFolder' and 'IterateHUDFileProperties' will write the properties to this
                var hudFolders = new Dictionary<string, dynamic>();

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    var userSetting = Settings.GetSetting(control.Name);
                    if (userSetting is not null)
                        WriteToFile(control, userSetting, hudFolders);
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
                                            // Initialise hudContainer and create inner Dictionary
                                            //  to contain elements specified
                                            hudContainer = new Dictionary<string, dynamic> {[key] = folder[property]};

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

                HUDBackground.Apply();

                return true;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        ///     Copy files used for folder-based customizations.
        /// </summary>
        public bool MoveCustomizationFiles(Dictionary<string, Controls[]>.ValueCollection hudSettings)
        {
            try
            {
                // Check if the customization folders exist.
                var path = $"{MainWindow.HudPath}\\{Name}\\";
                if (!Directory.Exists($"{path}\\{CustomizationsFolder}") &&
                    !Directory.Exists($"{path}\\{EnabledFolder}")) return true;

                // Get user's settings for the selected HUD.
                var userSettings = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(HUDSettings.UserFile))
                    ?.Settings.Where(x => x.HUD == Name);

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    // Loop through every control on the page, find the matching user setting.
                    var setting = userSettings.First(x => x.Name == control.Name);
                    if (setting is null) continue; // User setting not found, skipping.

                    var custom = path + CustomizationsFolder;
                    var enabled = path + EnabledFolder;

                    switch (control.Type.ToLowerInvariant())
                    {
                        case "checkbox":
                            var fileName = Utilities.GetFileNames(control);
                            if (fileName is null or not string) continue; // File name not found, skipping.

                            custom += $"\\{fileName}";
                            enabled += $"\\{fileName}";

                            // If true, move the customization file into the enabled folder, otherwise move it back.
                            if (string.Equals(setting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (Directory.Exists(custom))
                                    Directory.Move(custom, enabled);
                                else if (File.Exists(custom + ".res"))
                                    File.Move(custom + ".res", enabled + ".res", true);
                            }
                            else
                            {
                                if (Directory.Exists(enabled))
                                    Directory.Move(enabled, custom);
                                else if (File.Exists(enabled + ".res"))
                                    File.Move(enabled + ".res", custom + ".res", true);
                            }

                            break;

                        case "dropdown":
                        case "dropdownmenu":
                        case "select":
                        case "combobox":
                            var fileNames = Utilities.GetFileNames(control);
                            if (fileNames is null or not string[]) continue; // File names not found, skipping.

                            // Move every file assigned to this control back to the customization folder first.
                            foreach (string file in fileNames)
                            {
                                var name = file.Replace(".res", string.Empty);
                                if (Directory.Exists(enabled + $"\\{name}"))
                                    Directory.Move(enabled + $"\\{name}", custom + $"\\{name}");
                                else if (File.Exists(enabled + $"\\{name}.res"))
                                    File.Move(enabled + $"\\{name}.res", custom + $"\\{name}.res", true);
                            }

                            // Only move the files for the control option selected by the user.
                            if (!string.Equals(setting.Value, "0"))
                            {
                                var name = control.Options[int.Parse(setting.Value)].FileName;
                                if (string.IsNullOrWhiteSpace(name)) break;

                                name = name.Replace(".res", string.Empty);
                                if (Directory.Exists(custom + $"\\{name}"))
                                    Directory.Move(custom + $"\\{name}", enabled + $"\\{name}");
                                else if (File.Exists(custom + $"\\{name}.res"))
                                    File.Move(custom + $"\\{name}.res", enabled + $"\\{name}.res", true);
                            }

                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
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
                var (Files, Special) = GetControlInfo(hudSetting, userSetting);

                // Check for special cases like stock or custom backgrounds.
                if (Special is not null)
                {
                    // Assume the value of any customization that references 'special' is a bool
                    var enable = string.Equals(userSetting.Value, "True", StringComparison.CurrentCultureIgnoreCase);

                    // If the control is a ComboBox, compare the user value against the default item index.
                    if (string.Equals(userSetting.Type, "ComboBox", StringComparison.CurrentCultureIgnoreCase))
                        enable = !string.Equals(userSetting.Value, "0");

                    EvaluateSpecial(Special, userSetting, enable);
                }

                if (Files == null) return;

                // Applies $value (and handles keywords where applicable) to provided HUD element
                // JObject and returns a HUD element Dictionary, recursively
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
                            if (string.Equals(userSetting.Type, "ColorPicker",
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                // If the color is supposed to have a pulse, set the pulse value in the schema.
                                if (hudSetting.Pulse)
                                {
                                    var pulseKey = property.Key + "Pulse";
                                    hudElement[pulseKey] = Utilities.GetPulsedColor(userSetting.Value);
                                }

                                // If the color value is for an item rarity, update the dimmed and grayed values.
                                foreach (var value in Utilities.ItemRarities)
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

                // # Applies animation options to .txt file and handles keywords where applicable.
                //
                // This method takes a JObject of type <string, tuple | Animation> and applies
                // each keyword to the provided .txt file
                //
                // keywords:
                //     replace    takes a tuple of [true, false] values, evaluates $value and
                //                replaces text in the file
                //
                //     comment    takes an array of strings, and adds two forward slashes before
                //                each line that contains the any of the strings
                //
                //     uncomment  takes an array of strings, and removes te two forward slashes
                //                before each line that contains the any of the strings
                //
                // If the JObject property does not match a keyword, it is assumed to be an event
                // name and List of HUD Animations, in which case the method will parse the animation
                // file and overwrite the provided event animations with the JObject property's event
                // animations
                //
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
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = valid
                                            ? Utilities.CommentTextLine(lines[index])
                                            : Utilities.UncommentTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = Utilities.CommentTextLine(lines[index]);
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
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = valid
                                            ? Utilities.UncommentTextLine(lines[index])
                                            : Utilities.CommentTextLine(lines[index]);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = Utilities.UncommentTextLine(lines[index]);
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

                                animations ??= HUDAnimations.Parse(File.ReadAllText(filePath));

                                // Create new event or animation statements could stack
                                // over multiple 'apply customisations'
                                animations[animationOption.Key] = new List<HUDAnimation>();

                                foreach (var _animation in animationOption.Value.ToArray())
                                {
                                    var animation = _animation.ToObject<Dictionary<string, dynamic>>();

                                    // Create temporary variable to store current animation instead of adding directly in switch case
                                    // because there are conditional properties that might need to be added later
                                    //
                                    // Initialize to dynamic so type checker does not return HUDAnimation
                                    // for setting freq/gain/bias
                                    //
                                    dynamic current;

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
                                        if (string.Equals(current.Interpolator, "pulse",
                                            StringComparison.CurrentCultureIgnoreCase))
                                            current.Frequency = animation["Frequency"];

                                        if (string.Equals(current.Interpolator, "gain",
                                                StringComparison.CurrentCultureIgnoreCase) ||
                                            string.Equals(current.Interpolator, "bias",
                                                StringComparison.CurrentCultureIgnoreCase))
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

                foreach (var filePath in Files)
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
                        // Assume .txt is always an animation file (may cause issues with mod_textures.txt but assume we are only editing hud files)
                        WriteAnimationCustomizations(currentFilePath,
                            filePath.Value.ToObject<JObject>());
                    }
                    else
                    {
                        MainWindow.ShowMessageBox(MessageBoxImage.Error,
                            $"Could not recognize file extension '{extension}'");
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
            }
        }

        private (JObject, string) GetControlInfo(Controls hudSetting, Setting userSetting)
        {
            if (!string.Equals(hudSetting.Type, "ComboBox", StringComparison.CurrentCultureIgnoreCase))
                return (hudSetting.Files, hudSetting.Special);
            // Determine files using the files of the selected item's label or value
            // Could cause issues if label and value are both numbers but numbered differently
            var selected =
                hudSetting.Options.First(x => x.Label == userSetting.Value || x.Value == userSetting.Value);
            return (selected.Files, selected.Special);
        }

        /// <summary>
        ///     Check whether a control change requires a game restart.
        /// </summary>
        public void CheckIsDirty(Controls control)
        {
            if (control.Restart && !string.Equals(control.Value, Settings.GetSetting(control.Name).Value) && !DirtyControls.Contains(control.Label))
                DirtyControls.Add(control.Label);
            else
                DirtyControls.Remove(control.Label);
        }

        #region CUSTOM SETTINGS

        private void EvaluateSpecial(string Special, Setting userSetting, bool enable)
        {
            // Check for special conditions, namely if we should enable stock backgrounds.

            if (string.Equals(Special, "StockBackgrounds", StringComparison.CurrentCultureIgnoreCase))
                SetStockBackgrounds(MainWindow.HudPath + "\\" + Name + "\\materials\\console", enable);

            if (string.Equals(Special, "CustomBackground", StringComparison.CurrentCultureIgnoreCase))
                SetCustomBackground(userSetting.Value, enable);

            if (string.Equals(Special, "TransparentViewmodels", StringComparison.CurrentCultureIgnoreCase))
                CopyTransparentViewmodelAddon(enable);
        }

        /// <summary>
        ///     Toggle default backgrounds by renaming their file extensions.
        /// </summary>
        public bool SetStockBackgrounds(string imagePath, bool enable)
        {
            HUDBackground.SetStockBackgrounds(enable);
            return true;
        }

        /// <summary>
        ///     Generate a VTF background using an image provided by the user.
        /// </summary>
        public bool SetCustomBackground(string imagePath, bool enable)
        {
            HUDBackground.SetCustomBackground(imagePath, enable);
            return true;
        }

        /// <summary>
        ///     Copy configuration file for transparent viewmodels into the HUD's cfg folder.
        /// </summary>
        public static bool CopyTransparentViewmodelAddon(bool enable = false)
        {
            try
            {
                // Copy the config file required for this feature
                if (!enable || Process.GetProcessesByName("hl2").Any()) return true;
                File.Copy(
                    Directory.GetCurrentDirectory() + "\\Resources\\mastercomfig-transparent-viewmodels-addon.vpk",
                    MainWindow.HudPath + "\\mastercomfig-transparent-viewmodels-addon.vpk", true);
                return true;
            }
            catch (Exception e)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, $"{Resources.error_transparent_vm} {e.Message}");
                return false;
            }
        }

        #endregion
    }
}