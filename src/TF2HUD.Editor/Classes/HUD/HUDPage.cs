using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HUDEditor.Models;
using HUDEditor.Properties;
using Xceed.Wpf.Toolkit;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using GroupBox = System.Windows.Controls.GroupBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;

namespace HUDEditor.Classes
{
    public partial class HUD
    {
        /// <summary>
        ///     Generate the page layout using controls defined in the HUD schema.
        /// </summary>
        public Grid GetControls()
        {
            // Skip this process if the controls have already been rendered.
            if (isRendered) return Controls;

            (Grid container, Grid titleContainer) = DefineContainer();
            DefinePresetButtons(container, titleContainer);
            (ChildWindow preview, Image previewImage) = CreatePreviewModal(container);

            // NOTE: ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic for each section.
            dynamic sectionsContainer = ConfigureSectionsContainer();

            GenerateControlSections(container, preview, previewImage, sectionsContainer);

            isRendered = true;
            return Controls;
        }

        private void GenerateControlSections(Grid container, ChildWindow preview, Image previewImage, dynamic sectionsContainer)
        {
            var lastMargin = new Thickness(10, 2, 0, 0);
            var lastTop = lastMargin.Top;
            var groupBoxIndex = 0;

            // Generate each control section as defined in the schema.
            foreach (var section in ControlOptions.Keys)
            {
                GenerateControlSection(preview, previewImage, sectionsContainer, lastTop, groupBoxIndex, section);
                groupBoxIndex++;
            }

            container.Children.Add(sectionsContainer);
            Controls.Children.Add(container);
        }

        private void GenerateControlSection(ChildWindow preview, Image previewImage, dynamic sectionsContainer, double lastTop, int groupBoxIndex, string section)
        {
            var sectionContainer = new GroupBox
            {
                Header = section
            };

            var sectionContentContainer = new Grid();
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition());
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            CreateResetButton(section, sectionContentContainer);

            Panel sectionContent = Layout is not null ? new WrapPanel() : new StackPanel();
            sectionContent.Margin = new Thickness(3);

            GenerateControlsAndAddToSettings(preview, previewImage, lastTop, section, sectionContent);

            sectionContainer.Content = sectionContent;

            ConfigureSectionLayout(groupBoxIndex, section, sectionContainer);

            CreateScrollViewer(sectionContainer, sectionContentContainer);

            sectionContainer.Content = sectionContentContainer;
            sectionsContainer.Children.Add(sectionContainer);
        }

        private void ConfigureSectionLayout(int groupBoxIndex, string section, GroupBox sectionContainer)
        {
            if (Layout is not null)
            {
                // Avoid evaluating unnecessarily
                var groupBoxItemEvaluated = false;

                for (var i = 0; i < Layout.Length; i++)
                    for (var j = 0; j < Layout[i].Length; j++)
                    {
                        // Allow index and grid area for grid coordinates.
                        if (groupBoxIndex.ToString() == Layout[i][j] && !groupBoxItemEvaluated)
                        {
                            // Don't set column or row if it has already been set.
                            // Setting the column/row every time will break spans.
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
                            foreach (var sections in Layout)
                                if (groupBoxIndex.ToString() == sections[j] ||
                                    section == sections[j])
                                    rowSpan++;

                            Grid.SetRowSpan(sectionContainer, rowSpan);

                            // Break parent loop
                            groupBoxItemEvaluated = true;
                            break;
                        }

                        if (groupBoxItemEvaluated) break;
                    }
            }
        }

        private void GenerateControlsAndAddToSettings(ChildWindow preview, Image previewImage, double lastTop, string section, Panel sectionContent)
        {
            // Generate each individual control, add it to user settings.
            foreach (var controlItem in ControlOptions[section])
            {
                // WPF Control Names cannot start with numbers, add prefix
                var controlId = _utilities.EncodeID(controlItem.Name);
                var label = controlItem.Label;
                var tooltip = controlItem.Tooltip;
                Settings.AddSetting(controlId, controlItem);

                switch (controlItem.Type.ToLowerInvariant())
                {
                    case "checkbox":
                        CreateCheckbox(preview, previewImage, lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    case "color":
                    case "colour":
                    case "colorpicker":
                    case "colourpicker":
                        CreateColorControl(preview, previewImage, lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    case "dropdown":
                    case "dropdownmenu":
                    case "select":
                    case "combobox":
                        // Do not create a ComboBox if there are no defined options.
                        if (controlItem.Options is not { Length: > 0 }) break;
                        CreateDropdown(preview, previewImage, lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    case "number":
                    case "integer":
                    case "integerupdown":
                        CreateNumberControl(preview, previewImage, lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    case "crosshair":
                    case "customcrosshair":
                        CreateCrosshairDropdown(preview, previewImage, lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    case "background":
                    case "custombackground":
                        CreateBackgroundSelector(lastTop, sectionContent, controlItem, controlId, tooltip);
                        break;

                    case "text":
                    case "textbox":
                        CreateTextbox(lastTop, sectionContent, controlItem, controlId, label, tooltip);
                        break;

                    default:
                        throw new Exception($"Entered type {controlItem.Type} is invalid.");
                }
            }
        }

        private void CreateTextbox(double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {
            // Create the Control.
            var textContainer = new StackPanel
            {
                Margin = new Thickness(10, lastTop, 0, 10)
            };
            var textLabel = new Label
            {
                Content = label,
                FontSize = 18
            };
            var textInput = new TextBox
            {
                Name = id,
                Width = 270,
                Text = Settings.GetSetting<string>(controlItem.Name) ?? string.Empty,
                ToolTip = tooltip
            };

            // Override the control width, if set.
            if (controlItem.Width > 0)
            {
                textLabel.Width = controlItem.Width;
                textInput.Width = controlItem.Width;
            }

            // Add Events.
            textInput.LostFocus += (_, _) =>
            {
                Settings.SetSetting(textInput.Name, textInput.Text);
                CheckIsDirty(controlItem);
            };

            // Add to Page.
            textContainer.Children.Add(textLabel);
            textContainer.Children.Add(textInput);
            sectionContent.Children.Add(textContainer);
            controlItem.Control = textInput;
            _logger.Info($"Added textbox to the page ({textInput.Name}).");
        }

        private void CreateBackgroundSelector(double lastTop, Panel sectionContent, Controls controlItem, string id, string tooltip)
        {
            var bgContainer = new Grid
            {
                Margin = new Thickness(10, lastTop + 5, 0, 0),
                ToolTip = tooltip
            };

            bgContainer.ColumnDefinitions.Add(new ColumnDefinition());
            bgContainer.ColumnDefinitions.Add(new ColumnDefinition());
            bgContainer.RowDefinitions.Add(new RowDefinition());
            bgContainer.RowDefinitions.Add(new RowDefinition());
            bgContainer.RowDefinitions.Add(new RowDefinition());

            var bgLabel = new Label
            {
                Content = controlItem.Label,
                FontSize = 18
            };
            Grid.SetColumn(bgLabel, 0);
            Grid.SetColumnSpan(bgLabel, 2);
            Grid.SetRow(bgLabel, 0);

            // Create the Control.
            var bgInput = new Button
            {
                Name = id,
                Content = _localization.UiBrowse,
                // Width = 100,
                Height = 32,
                Padding = new Thickness(5, 2, 5, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Grid.SetColumn(bgInput, 0);
            Grid.SetRow(bgInput, 1);

            var clearInput = new Button
            {
                Content = _localization.UiClear,
                // Width = 100,
                Height = 32,
                Padding = new Thickness(5, 2, 5, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Grid.SetColumn(clearInput, 1);
            Grid.SetRow(clearInput, 1);

            // Add preview image
            var bgImage = new Image
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Grid.SetColumn(bgImage, 0);
            Grid.SetColumnSpan(bgImage, 2);
            Grid.SetRow(bgImage, 2);

            // Add Events.
            bgInput.Click += (_, _) =>
            {
                _notifier.ShowMessageBox(MessageBoxImage.Information, _localization.InfoBackgroundOverride);
                using (var browser = new OpenFileDialog())
                {
                    browser.ShowDialog();
                    if (string.IsNullOrWhiteSpace(browser.FileName)) return;

                    var path = $"{System.Windows.Forms.Application.LocalUserAppDataPath}\\Images\\{browser.FileName.Split('\\')[^1]}";

                    bgImage.Source = new BitmapImage(new Uri(browser.FileName));

                    Settings.SetSetting(controlItem.Name, path);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    _logger.Info($"Copying {browser.FileName} to {path}");
                    File.Copy(browser.FileName, path, true);
                }

                CheckIsDirty(controlItem);
            };

            clearInput.Click += (_, _) =>
            {
                bgImage.Source = null;
                Settings.SetSetting(controlItem.Name, "");
            };

            var imageSource = Settings.GetSetting<string>(controlItem.Name);

            if (!string.IsNullOrWhiteSpace(imageSource))
                if (Uri.TryCreate(imageSource, UriKind.Absolute, out var path))
                    bgImage.Source = new BitmapImage(path);

            // Add to Page.
            bgContainer.Children.Add(bgLabel);
            bgContainer.Children.Add(bgInput);
            bgContainer.Children.Add(clearInput);
            bgContainer.Children.Add(bgImage);
            sectionContent.Children.Add(bgContainer);
            controlItem.Control = bgInput;
            _logger.Info($"Added background selector to the page ({bgInput.Name}).");
        }

        private void CreateCrosshairDropdown(ChildWindow preview, Image previewImage, double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {
            // Create the Control.
            var xhairContainer = new StackPanel
            {
                Margin = new Thickness(10, lastTop, 0, 10)
            };
            var xhairLabel = new Label
            {
                Content = label,
                Style = (Style)Application.Current.Resources["CrosshairLabel"]
            };
            var xhairInput = new ComboBox
            {
                Name = id,
                ToolTip = tooltip
            };

            // Add items to the ComboBox.
            foreach (var item in _utilities.CrosshairStyles().Select(option => new ComboBoxItem
            {
                Content = option,
                Style = (Style)Application.Current.Resources["Crosshair"]
            }))
            {
                xhairInput.Style = (Style)Application.Current.Resources["CrosshairBox"];
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
            _logger.Info($"Added xhair picker to the page ({xhairInput.Name}).");

            // Create a preview button if the control has a preview image.
            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
            {
                var previewBtn = new Button
                {
                    Style = (Style)Application.Current.Resources["PreviewButton"],
                    Margin = new Thickness(0, lastTop, 0, 0)
                };
                previewBtn.Click += (_, _) =>
                {
                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                        ? tooltip
                        : id;
                    previewImage.Source = new BitmapImage(new Uri(controlItem.Preview));
                    preview.Show();
                };
                sectionContent.Children.Add(previewBtn);
                _logger.Info($"{xhairInput.Name} - Added preview: {controlItem.Preview}.");
            }
        }

        private void CreateNumberControl(ChildWindow preview, Image previewImage, double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {
            // Create the Control.
            var integerContainer = new StackPanel
            {
                Margin = new Thickness(10, lastTop, 0, 10)
            };
            var integerLabel = new Label
            {
                Content = label,
                Style = (Style)Application.Current.Resources["IntegerUpDownLabel"]
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

            // Override the control width, if set.
            if (controlItem.Width > 0)
            {
                integerLabel.Width = controlItem.Width;
                integerInput.Width = controlItem.Width;
            }

            // Add Events.
            integerInput.ValueChanged += (sender, _) =>
            {
                var input = sender as IntegerUpDown;
                Settings.SetSetting(input?.Name, input?.Text);
                CheckIsDirty(controlItem);
            };

            // Add to Page.
            integerContainer.Children.Add(integerLabel);
            integerContainer.Children.Add(integerInput);
            sectionContent.Children.Add(integerContainer);
            controlItem.Control = integerInput;
            _logger.Info($"Added num. counter to the page ({integerInput.Name}).");

            // Create a preview button if the control has a preview image.
            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
            {
                var previewBtn = new Button
                {
                    Style = (Style)Application.Current.Resources["PreviewButton"],
                    Margin = new Thickness(0, lastTop, 0, 0)
                };
                previewBtn.Click += (_, _) =>
                {
                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                        ? tooltip
                        : id;
                    previewImage.Source = new BitmapImage(new Uri(controlItem.Preview));
                    preview.Show();
                };
                sectionContent.Children.Add(previewBtn);
                _logger.Info($"{integerInput.Name} - Added preview: {controlItem.Preview}.");
            }
        }

        private void CreateDropdown(ChildWindow preview, Image previewImage, double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {


            // Create the Control.
            var comboBoxContainer = new StackPanel
            {
                Margin = new Thickness(10, lastTop, 0, 5)
            };
            var comboBoxLabel = new Label
            {
                Content = label,
                Style = (Style)Application.Current.Resources["ComboBoxLabel"]
            };
            var comboBoxInput = new ComboBox
            {
                Name = id,
                ToolTip = tooltip,
                Width = 150
            };

            // Override the control width, if set.
            if (controlItem.Width > 0)
            {
                comboBoxLabel.Width = controlItem.Width;
                comboBoxInput.Width = controlItem.Width;
            }

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
            _logger.Info($"Added combo-box to the page ({comboBoxInput.Name}).");

            // Create a preview button if the control has a preview image.
            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
            {
                var previewBtn = new Button
                {
                    Style = (Style)Application.Current.Resources["PreviewButton"],
                    Margin = new Thickness(0, lastTop, 0, 0)
                };
                previewBtn.Click += (_, _) =>
                {
                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                        ? tooltip
                        : id;
                    previewImage.Source = new BitmapImage(new Uri(controlItem.Preview));
                    preview.Show();
                };
                sectionContent.Children.Add(previewBtn);
                _logger.Info($"{comboBoxInput.Name} - Added preview: {controlItem.Preview}.");
            }
        }

        private void CreateColorControl(ChildWindow preview, Image previewImage, double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {
            // Create the Control.
            var colorContainer = new StackPanel
            {
                Margin = new Thickness(10, lastTop, 0, 10)
            };
            var colorLabel = new Label
            {
                Content = label,
                Style = (Style)Application.Current.Resources["ColorPickerLabel"]
            };
            var colorInput = new ColorPicker
            {
                Name = id,
                ToolTip = tooltip
            };

            // Override the control width, if set.
            if (controlItem.Width > 0)
            {
                colorLabel.Width = controlItem.Width;
                colorInput.Width = controlItem.Width;
            }

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
                    _utilities.ConvertToRgba(input?.SelectedColor.ToString()));
            };
            colorInput.Closed += (sender, _) =>
            {
                var input = sender as ColorPicker;
                Settings.SetSetting(input?.Name,
                    _utilities.ConvertToRgba(input?.SelectedColor.ToString()));
                CheckIsDirty(controlItem);
            };

            // Add to Page.
            colorContainer.Children.Add(colorLabel);
            colorContainer.Children.Add(colorInput);
            sectionContent.Children.Add(colorContainer);
            controlItem.Control = colorInput;
            _logger.Info($"Added color picker to the page ({colorInput.Name}).");

            // Create a preview button if the control has a preview image.
            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
            {
                var previewBtn = new Button
                {
                    Style = (Style)Application.Current.Resources["PreviewButton"],
                    Margin = new Thickness(5, lastTop, 0, 0)
                };
                previewBtn.Click += (_, _) =>
                {
                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                        ? tooltip
                        : id;
                    previewImage.Source = new BitmapImage(new Uri(controlItem.Preview));
                    preview.Show();
                };
                sectionContent.Children.Add(previewBtn);
                _logger.Info($"{colorInput.Name} - Added preview: {controlItem.Preview}.");
            }
        }

        private void CreateCheckbox(ChildWindow preview, Image previewImage, double lastTop, Panel sectionContent, Controls controlItem, string id, string label, string tooltip)
        {
            // Create the Control.
            var checkBoxInput = new CheckBox
            {
                Name = id,
                Content = label,
                Margin = new Thickness(10, lastTop + 10, 30, 0),
                IsChecked = Settings.GetSetting<bool>(id),
                ToolTip = tooltip
            };

            // Override the control width, if set.
            if (controlItem.Width > 0)
                checkBoxInput.Width = controlItem.Width;

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
            _logger.Info($"Added checkbox to the page ({checkBoxInput.Name}).");

            // Create a preview button if the control has a preview image.
            if (!string.IsNullOrWhiteSpace(controlItem.Preview))
            {
                var previewBtn = new Button
                {
                    Style = (Style)Application.Current.Resources["PreviewButton"],
                    Margin = new Thickness(0, lastTop, 0, 0),
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                previewBtn.Click += (_, _) =>
                {
                    preview.Caption = !string.IsNullOrWhiteSpace(tooltip) ? tooltip : id;
                    previewImage.Source = new BitmapImage(new Uri(controlItem.Preview));
                    preview.Show();
                };
                sectionContent.Children.Add(previewBtn);
                _logger.Info($"{checkBoxInput.Name} - Added preview: {controlItem.Preview}.");
            }
        }

        private static void CreateScrollViewer(GroupBox sectionContainer, Grid sectionContentContainer)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = sectionContainer.Content,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            Grid.SetRow(scrollViewer, 0);
            sectionContentContainer.Children.Add(scrollViewer);
        }

        private void CreateResetButton(string section, Grid sectionContentContainer)
        {
            // Create the reset button for each control section.
            var resetInput = new Button
            {
                Style = (Style)Application.Current.Resources["PreviewButton"],
                HorizontalAlignment = HorizontalAlignment.Right,
                Content = ".",
                Opacity = 0.4
            };

            resetInput.MouseEnter += (_, _) => resetInput.Opacity = 1;
            resetInput.MouseLeave += (_, _) => resetInput.Opacity = 0.4;

            Grid.SetColumn(resetInput, 1);

            resetInput.Click += (_, _) =>
            {
                if (!MainWindow.HudSelection.Equals(Name)) return;
                ResetSection(section);
                Settings.SaveSettings();
                if (MainWindow.CheckHudInstallation()) ApplyCustomizations();
                DirtyControls.Clear();
            };

            sectionContentContainer.Children.Add(resetInput);
        }

        private dynamic ConfigureSectionsContainer()
        {
            dynamic sectionsContainer;

            if (LayoutOptions is not null)
            {
                // Splits Layout string[] into 2D Array using \s+
                Layout = LayoutOptions.Select(t => Regex.Split(t, "\\s+")).ToArray();

                sectionsContainer = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    MaxWidth = 1280,
                    MaxHeight = 720
                };

                // Assume that all row arrays are the same length, use column information from Layout[0].
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

            return sectionsContainer;
        }

        private static (ChildWindow, Image) CreatePreviewModal(Grid container)
        {
            var preview = new ChildWindow
            {
                Style = (Style)Application.Current.Resources["PreviewPanel"]
            };
            preview.MouseDoubleClick += (_, _) => { preview.Close(); };

            var previewImage = new Image
            {
                Style = (Style)Application.Current.Resources["PreviewImage"]
            };
            preview.Content = previewImage;

            container.Children.Add(preview);

            return (preview, previewImage);
        }

        /// <summary>
        /// Create preset buttons
        /// </summary>
        /// <param name="container"></param>
        /// <param name="titleContainer"></param>
        private void DefinePresetButtons(Grid container, Grid titleContainer)
        {
            var presetsContainer = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 5)
            };

            foreach (var preset in Enum.GetValues<HUDSettingsPreset>())
            {
                var presetButton = new Button
                {
                    Style = (Style)Application.Current.Resources["HUDButton"],
                    Content = preset,
                    FontSize = 25,
                    Width = 35,
                    Height = 35,
                    Margin = new Thickness(1, 0, 1, 0)
                };

                if (Settings.Preset == preset)
                {
                    // LighterRed
                    var colors = Array.ConvertAll("220 100 80 255".Split(' '), byte.Parse);
                    presetButton.Background = new SolidColorBrush(Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]));
                }

                presetButton.Click += (_, _) =>
                {
                    // Settings.Preset = Enum.Parse<Classes.HUDSettingsPreset>(presetButton.Name.Split('_')[^1]);
                    Settings.Preset = preset;
                    _logger.Info($"Changed preset for {Name} to HUDSettingsPreset.{Settings.Preset}");

                    isRendered = false;
                    Controls = new Grid();
                    PresetChanged.Invoke(this, Settings.Preset);
                };
                presetsContainer.Children.Add(presetButton);
            }

            Grid.SetColumn(presetsContainer, 1);
            titleContainer.Children.Add(presetsContainer);
            container.Children.Add(titleContainer);
        }

        private (Grid, Grid) DefineContainer()
        {
            // Define the container that will hold the title and content.
            var container = new Grid();
            var titleContainer = new Grid { VerticalAlignment = VerticalAlignment.Center };
            for (var i = 0; i < 3; i++)
                titleContainer.ColumnDefinitions.Add(new ColumnDefinition());

            var contentRow = new RowDefinition();
            if (Layout is not null) contentRow.Height = GridLength.Auto;
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(contentRow);

            return (container, titleContainer);
        }

        /// <summary>
        ///     Check whether a control change requires a game restart.
        /// </summary>
        public void CheckIsDirty(Controls control)
        {
            if (control.Restart && !string.Equals(control.Value, Settings.GetSetting(control.Name).Value) &&
                !DirtyControls.Contains(control.Label))
                DirtyControls.Add(control.Label);
            else
                DirtyControls.Remove(control.Label);
        }
    }
}