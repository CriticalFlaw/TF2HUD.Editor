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
                    MaxWidth = 1280,
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
                                Margin = new Thickness(10, lastTop + 10, 50, 0),
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
                            MainWindow.Logger.Info($"Added checkbox to the page ({checkBoxInput.Name}).");

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
                                MainWindow.Logger.Info($"{checkBoxInput.Name} - Added preview: {controlItem.Preview}.");
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
                            MainWindow.Logger.Info($"Added color picker to the page ({colorInput.Name}).");

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
                                MainWindow.Logger.Info($"{colorInput.Name} - Added preview: {controlItem.Preview}.");
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
                            MainWindow.Logger.Info($"Added combo-box to the page ({comboBoxInput.Name}).");

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
                                MainWindow.Logger.Info($"{comboBoxInput.Name} - Added preview: {controlItem.Preview}.");
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
                            MainWindow.Logger.Info($"Added num. counter to the page ({integerInput.Name}).");

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
                                MainWindow.Logger.Info($"{integerInput.Name} - Added preview: {controlItem.Preview}.");
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
                            MainWindow.Logger.Info($"Added xhair picker to the page ({xhairInput.Name}).");

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
                                MainWindow.Logger.Info($"{xhairInput.Name} - Added preview: {controlItem.Preview}.");
                            }

                            break;

                        case "background":
                        case "custombackground":

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
                                Content = "Browse",
                                // Width = 100,
                                Height = 32,
                                Padding = new Thickness(5, 2, 5, 0),
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };

                            Grid.SetColumn(bgInput, 0);
                            Grid.SetRow(bgInput, 1);

                            var clearInput = new Button
                            {
                                Content = "Reset",
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
                                using (var fbd = new OpenFileDialog())
                                {
                                    fbd.ShowDialog();
                                    if (string.IsNullOrWhiteSpace(fbd.FileName)) return;
                                    var path =
                                        $"{System.Windows.Forms.Application.LocalUserAppDataPath}\\Images\\{fbd.FileName.Split('\\')[^1]}";

                                    bgImage.Source = new BitmapImage(new Uri(fbd.FileName));

                                    Settings.SetSetting(controlItem.Name, path);

                                    MainWindow.Logger.Info($"Copying {fbd.FileName} to {path}");

                                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                                    File.Copy(fbd.FileName, path, true);
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
                            MainWindow.Logger.Info($"Added background selector to the page ({bgInput.Name}).");
                            break;

                        case "text":
                        case "textbox":
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
                                Text = controlItem.Value ?? string.Empty,
                                ToolTip = tooltip
                            };

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
                            MainWindow.Logger.Info($"Added textbox to the page ({textInput.Name}).");
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

                sectionContainer.Content = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = sectionContainer.Content,
                    Background = new SolidColorBrush(Colors.Transparent)
                };
                sectionsContainer.Children.Add(sectionContainer);
                groupBoxIndex++;
            }

            container.Children.Add(sectionsContainer);
            controls.Children.Add(container);

            isRendered = true;
            return controls;
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
