using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using System.Windows;
using Thickness = Avalonia.Thickness;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using GridLength = Avalonia.Controls.GridLength;
using Style = Avalonia.Styling.Style;
using Avalonia.Media.Imaging;

namespace HUDEdit.Classes;

public partial class HUD
{
    /// <summary>
    /// Generates the page layout using controls defined in the HUD schema.
    /// </summary>
    public Grid GetControls()
    {
        // Skip this process if the controls have already been rendered.
        if (IsRendered) return Controls;

        // Define the container that will hold the title and content.
        var container = new Grid();

        var contentRow = new RowDefinition();
        if (Layout is not null) contentRow.Height = GridLength.Auto;
        container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        container.RowDefinitions.Add(contentRow);

        // TODO: Create the preview modal
        //var preview = new Avalonia.Controls.ChildWindow();
        //preview.MouseDoubleClick += (_, _) => { preview.Close(); };

        //var image = new Image();
        //image.Classes.Add("PreviewImage");
        //preview.Content = image;

        //container.Children.Add(preview);

        // NOTE: ColumnDefinition and RowDefinition only exist on Grid, not Panel, so we are forced to use dynamic for each section.
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
        var lastMargin = new Thickness(10, 2, 0, 0);
        var lastTop = lastMargin.Top;
        var groupBoxIndex = 0;

        // Generate each control section as defined in the schema.
        foreach (var section in ControlOptions.Keys)
        {
            var sectionContainer = new HeaderedContentControl
            {
                Header = section
            };

            var sectionContentContainer = new Grid();
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition());
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Create the reset button for each control section.
            var resetInput = new Button
            {
                //Style = (Style)Application.Current.Resources["PreviewButton"],
                HorizontalAlignment = HorizontalAlignment.Right,
                Content = "\u0157",
                Opacity = 0.4
            };

            //resetInput.MouseEnter += (_, _) => resetInput.Opacity = 1;
            //resetInput.MouseLeave += (_, _) => resetInput.Opacity = 0.4;

            Grid.SetColumn(resetInput, 1);

            resetInput.Click += (_, _) =>
            {
                if (!MainWindow.HudSelection.Equals(Name)) return;
                ResetSection(section);
                Settings.SaveSettings();
                if (MainWindow.CheckHudInstallation(this)) ApplyCustomizations();
                DirtyControls.Clear();
            };

            sectionContentContainer.Children.Add(resetInput);
            Panel sectionContent = Layout is not null ? new WrapPanel() : new StackPanel();
            sectionContent.Margin = new Thickness(3);

            // Generate each individual control, add it to user settings.
            foreach (var controlItem in ControlOptions[section])
            {
                // WPF Control Names cannot start with numbers, add prefix
                var id = Utilities.EncodeId(controlItem.Name);
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
                            //ToolTip = tooltip
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

                        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && label.Contains("Toggle Crosshair"))
                            checkBoxInput.IsChecked = App.Config.ConfigSettings.UserPrefs.CrosshairEnabled;

                        // Add to Page.
                        sectionContent.Children.Add(checkBoxInput);
                        controlItem.Control = checkBoxInput;
                        App.Logger.Info($"Added {checkBoxInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            var previewBtn = new Button
                            {
                                Margin = new Thickness(0, lastTop, 0, 0),
                                VerticalAlignment = VerticalAlignment.Bottom
                            };
                            previewBtn.Classes.Add("PreviewButton");
                            previewBtn.Click += (_, _) =>
                            {
                                //preview.Caption = !string.IsNullOrWhiteSpace(tooltip) ? tooltip : id;
                                //image.Source = new Bitmap(new Uri(controlItem.Preview).ToString());
                                //preview.Show();
                            };
                            sectionContent.Children.Add(previewBtn);
                            App.Logger.Info($"Added a preview for {checkBoxInput.Name} ({controlItem.Preview})");
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
                        var colorLabel = new Label();
                        colorLabel.Content = label;
                        colorLabel.Classes.Add("ColorPickerLabel");
                        var colorInput = new Avalonia.Controls.ColorPicker
                        {
                            Name = id,
                            //ToolTip = tooltip
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
                            if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && label.Contains("Crosshair"))
                                colorInput.Color = Utilities.ConvertToColor(App.Config.ConfigSettings.UserPrefs.CrosshairColor);
                            else
                                colorInput.Color = Settings.GetSetting<Color>(id);
                        }
                        catch
                        {
                            colorInput.Color = Color.FromArgb(255, 0, 255, 0);
                        }

                        // Add Events.
                        colorInput.ColorChanged += (sender, _) =>
                        {
                            var input = sender as Avalonia.Controls.ColorPicker;
                            Settings.SetSetting(input?.Name, Utilities.ConvertToRgba(input?.Color.ToString()));
                        };
                        //colorInput.Closed += (sender, _) =>
                        //{
                        //    var input = sender as ColorPicker;
                        //    Settings.SetSetting(input?.Name, Utilities.ConvertToRgba(input?.SelectedColor.ToString()));
                        //    CheckIsDirty(controlItem);
                        //};

                        // Add to Page.
                        colorContainer.Children.Add(colorLabel);
                        colorContainer.Children.Add(colorInput);
                        sectionContent.Children.Add(colorContainer);
                        controlItem.Control = colorInput;
                        App.Logger.Info($"Added {colorInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            var previewBtn = new Button
                            {
                                Margin = new Thickness(5, lastTop, 0, 0)
                            };
                            previewBtn.Classes.Add("PreviewButton");
                            previewBtn.Click += (_, _) =>
                            {
                                //preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                //    ? tooltip
                                //    : id;
                                //image.Source = new Bitmap(new Uri(controlItem.Preview).ToString());
                                //preview.Show();
                            };
                            sectionContent.Children.Add(previewBtn);
                            App.Logger.Info($"Added a preview for {colorInput.Name} ({controlItem.Preview})");
                        }

                        break;

                    case "dropdown":
                    case "dropdownmenu":
                    case "select":
                    case "combobox":
                        // Do not create a ComboBox if there are no defined options.
                        if (controlItem.Options is not { Length: > 0 }) break;

                        // Create the Control.
                        var comboBoxContainer = new StackPanel
                        {
                            Margin = new Thickness(10, lastTop, 0, 5)
                        };
                        var comboBoxLabel = new Label
                        {
                            Content = label
                        };
                        comboBoxLabel.Classes.Add("ComboBoxLabel");
                        var comboBoxInput = new ComboBox
                        {
                            Name = id,
                            //ToolTip = tooltip,
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
                        App.Logger.Info($"Added {comboBoxInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            var previewBtn = new Button
                            {
                                Margin = new Thickness(0, lastTop, 0, 0)
                            };
                            previewBtn.Classes.Add("PreviewButton");
                            previewBtn.Click += (_, _) =>
                            {
                                //preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                //    ? tooltip
                                //    : id;
                                //image.Source = new Bitmap(new Uri(controlItem.Preview).ToString());
                                //preview.Show();
                            };
                            sectionContent.Children.Add(previewBtn);
                            App.Logger.Info($"Added a preview for {comboBoxInput.Name} ({controlItem.Preview})");
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
                            Content = label
                        };
                        integerLabel.Classes.Add("IntegerUpDownLabel");
                        var integerInput = new NumericUpDown
                        {
                            Name = id,
                            Value = Settings.GetSetting<int>(id),
                            Minimum = controlItem.Minimum,
                            Maximum = controlItem.Maximum,
                            Increment = controlItem.Increment,
                            //ToolTip = tooltip
                        };

                        // Override the control width, if set.
                        if (controlItem.Width > 0)
                        {
                            integerLabel.Width = controlItem.Width;
                            integerInput.Width = controlItem.Width;
                        }

                        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && label.Contains("Size"))
                            integerInput.Value = App.Config.ConfigSettings.UserPrefs.CrosshairSize;

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
                        App.Logger.Info($"Added {integerInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            var previewBtn = new Button
                            {
                                Margin = new Thickness(0, lastTop, 0, 0)
                            };
                            previewBtn.Classes.Add("PreviewButton");
                            previewBtn.Click += (_, _) =>
                            {
                                //preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                //    ? tooltip
                                //    : id;
                                //image.Source = new Bitmap(new Uri(controlItem.Preview).ToString());
                                //preview.Show();
                            };
                            sectionContent.Children.Add(previewBtn);
                            App.Logger.Info($"Added a preview for {integerInput.Name} ({controlItem.Preview})");
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
                            Content = label
                        };
                        xhairLabel.Classes.Add("CrosshairLabel");
                        var xhairInput = new ComboBox
                        {
                            Name = id,
                            //ToolTip = tooltip
                        };

                        // Add items to the ComboBox.
                        foreach (var item in Utilities.CrosshairStyles.Select(option => new ComboBoxItem
                        {
                            Content = option,
                            //Style = (Style)Application.Current.Resources["Crosshair"]
                        }))
                        {
                            xhairInput.Classes.Add("CrosshairBox");
                            xhairInput.Items.Add(item);
                        }

                        // Set the selected value depending on the what's retrieved from the setting file.
                        var xhairValue = Settings.GetSetting<string>(id);
                        if (!Regex.IsMatch(xhairValue, "\\D"))
                            xhairInput.SelectedIndex = int.Parse(xhairValue);
                        else
                            xhairInput.SelectedValue = xhairValue;

                        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && label.Contains("Style"))
                            xhairInput.SelectedValue = App.Config.ConfigSettings.UserPrefs.CrosshairStyle;

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
                        App.Logger.Info($"Added {xhairInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            var previewBtn = new Button
                            {
                                Margin = new Thickness(0, lastTop, 0, 0)
                            };
                            previewBtn.Classes.Add("PreviewButton");
                            previewBtn.Click += (_, _) =>
                            {
                                //preview.Caption = !string.IsNullOrWhiteSpace(tooltip)
                                //    ? tooltip
                                //    : id;
                                //image.Source = new Bitmap(new Uri(controlItem.Preview).ToString());
                                //preview.Show();
                            };
                            sectionContent.Children.Add(previewBtn);
                            App.Logger.Info($"Added a preview for {xhairInput.Name} ({controlItem.Preview})");
                        }

                        break;

                    case "background":
                    case "custombackground":

                        var bgContainer = new Grid
                        {
                            Margin = new Thickness(10, lastTop + 5, 0, 0),
                            //ToolTip = tooltip
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
                            Content = Localization.Resources.ui_browse,
                            Height = 32,
                            Padding = new Thickness(5, 2, 5, 0),
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        Grid.SetColumn(bgInput, 0);
                        Grid.SetRow(bgInput, 1);

                        var clearInput = new Button
                        {
                            Content = Localization.Resources.ui_clear,
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
                            MainWindow.ShowMessageBox(MessageBoxImage.Information, Localization.Resources.info_background_override);
                            //using (var browser = new OpenFileDialog())
                            //{
                            //    browser.ShowDialog();
                            //    if (string.IsNullOrWhiteSpace(browser.FileName)) return;

                            //    var path = $"{System.Windows.Forms.Application.LocalUserAppDataPath}\\Images\\{browser.FileName.Split('\\')[^1]}";

                            //    bgImage.Source = new Bitmap(new Uri(browser.FileName).ToString());

                            //    Settings.SetSetting(controlItem.Name, path);

                            //    if (!Directory.Exists(Path.GetDirectoryName(path)))
                            //        Directory.CreateDirectory(Path.GetDirectoryName(path));
                            //    App.Logger.Info($"Copying \"{browser.FileName}\" to \"{path}\"");
                            //    File.Copy(browser.FileName, path, true);
                            //}

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
                                bgImage.Source = new Bitmap(path.ToString());

                        // Add to Page.
                        bgContainer.Children.Add(bgLabel);
                        bgContainer.Children.Add(bgInput);
                        bgContainer.Children.Add(clearInput);
                        bgContainer.Children.Add(bgImage);
                        sectionContent.Children.Add(bgContainer);
                        controlItem.Control = bgInput;
                        App.Logger.Info($"Added {bgInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");
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
                            Text = Settings.GetSetting<string>(controlItem.Name) ?? string.Empty,
                            //ToolTip = tooltip
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
                        App.Logger.Info($"Added {textInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");
                        break;

                    default:
                        throw new Exception($"Entered type {controlItem.Type} is invalid");
                }
            }

            sectionContainer.Content = sectionContent;

            if (Layout is not null)
            {
                // Avoid evaluating unnecessarily
                var groupBoxItemEvaluated = false;

                for (var i = 0; i < Layout.Length; i++)
                {
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

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = sectionContainer.Content,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            Grid.SetRow(scrollViewer, 0);
            sectionContentContainer.Children.Add(scrollViewer);
            sectionContainer.Content = sectionContentContainer;
            sectionsContainer.Children.Add(sectionContainer);
            groupBoxIndex++;
        }

        var scrollView = new ScrollViewer()
        {
            Content = sectionsContainer
        };
        Grid.SetRow(scrollView, 1);
        container.Children.Add(scrollView);
        Controls.Children.Add(container);

        IsRendered = true;
        return Controls;
    }

    /// <summary>
    /// Checks whether a control change requires a game restart.
    /// </summary>
    private void CheckIsDirty(Shared.Models.Controls control)
    {
        if (control.Restart && !string.Equals(control.Value, Settings.GetSetting(control.Name).Value) && !DirtyControls.Contains(control.Label))
            DirtyControls.Add(control.Label);
        else
            DirtyControls.Remove(control.Label);
    }
}