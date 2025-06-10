using System;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Thickness = Avalonia.Thickness;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using GridLength = Avalonia.Controls.GridLength;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;

namespace HUDEdit.Classes;

public partial class HUD
{
    private readonly double lastTop = new Thickness(10, 2, 0, 0).Top;

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
            sectionsContainer = new WrapPanel();
            //sectionsContainer.Margin = new Thickness(10);
        }

        Grid.SetRow(sectionsContainer, 1);
        var groupBoxIndex = 0;

        // Generate each control section as defined in the schema.
        foreach (var section in ControlOptions.Keys)
        {
            var sectionContainer = new HeaderedContentControl();
            sectionContainer.Header = section;
            sectionContainer.Margin = new Thickness(5);

            var sectionContentContainer = new Grid();
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition());
            sectionContentContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sectionContentContainer.Children.Add(CreateResetButton(section));

            Panel sectionContent = Layout is not null ? new WrapPanel() : new StackPanel();

            // Generate each individual control, add it to user settings.
            foreach (var controlItem in ControlOptions[section])
            {
                // WPF Control Names cannot start with numbers, add prefix
                Settings.AddSetting(Utilities.EncodeId(controlItem.Name), controlItem);

                switch (controlItem.Type.ToLowerInvariant())
                {
                    case "checkbox":
                        var checkBoxInput = CreateCheckbox(controlItem);

                        // Add to page.
                        sectionContent.Children.Add(checkBoxInput);
                        controlItem.Control = checkBoxInput;
                        App.Logger.Info($"Added {checkBoxInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            sectionContent.Children.Add(CreatePreviewButton(controlItem.Preview));
                            App.Logger.Info($"Added a preview for {checkBoxInput.Name} ({controlItem.Preview})");
                        }
                        break;

                    case "color":
                    case "colour":
                    case "colorpicker":
                    case "colourpicker":
                        var colorContainer = CreateColorPicker(controlItem);
                        var colorInput = colorContainer.Children.OfType<Avalonia.Controls.ColorPicker>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(colorContainer);
                        controlItem.Control = colorInput;
                        App.Logger.Info($"Added {colorInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            sectionContent.Children.Add(CreatePreviewButton(controlItem.Preview));
                            App.Logger.Info($"Added a preview for {colorInput.Name} ({controlItem.Preview})");
                        }
                        break;

                    case "dropdown":
                    case "dropdownmenu":
                    case "select":
                    case "combobox":
                        // Do not create a ComboBox if there are no defined options.
                        if (controlItem.Options is not { Length: > 0 }) break;

                        var comboBoxContainer = CreateComboBox(controlItem);
                        var comboBoxInput = comboBoxContainer.Children.OfType<ComboBox>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(comboBoxContainer);
                        controlItem.Control = comboBoxInput;
                        App.Logger.Info($"Added {comboBoxInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            sectionContent.Children.Add(CreatePreviewButton(controlItem.Preview));
                            App.Logger.Info($"Added a preview for {comboBoxInput.Name} ({controlItem.Preview})");
                        }
                        break;

                    case "number":
                    case "integer":
                    case "integerupdown":
                        var integerContainer = CreateNumberPicker(controlItem);
                        var integerInput = integerContainer.Children.OfType<NumericUpDown>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(integerContainer);
                        controlItem.Control = integerInput;
                        App.Logger.Info($"Added {integerInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            sectionContent.Children.Add(CreatePreviewButton(controlItem.Preview));
                            App.Logger.Info($"Added a preview for {integerInput.Name} ({controlItem.Preview})");
                        }
                        break;

                    case "crosshair":
                    case "customcrosshair":
                        var xhairContainer = CreateCrosshairPicker(controlItem);
                        var xhairInput = xhairContainer.Children.OfType<ComboBox>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(xhairContainer);
                        controlItem.Control = xhairInput;
                        App.Logger.Info($"Added {xhairInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");

                        // Create a preview button if the control has a preview image.
                        if (!string.IsNullOrWhiteSpace(controlItem.Preview))
                        {
                            sectionContent.Children.Add(CreatePreviewButton(controlItem.Preview));
                            App.Logger.Info($"Added a preview for {xhairInput.Name} ({controlItem.Preview})");
                        }
                        break;

                    case "background":
                    case "custombackground":
                        var bgContainer = CreateBackgroundSetter(controlItem);
                        var bgInput = bgContainer.Children.OfType<Button>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(bgContainer);
                        controlItem.Control = bgInput;
                        App.Logger.Info($"Added {bgInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");
                        break;

                    case "text":
                    case "textbox":
                        var textContainer = CreateTextBox(controlItem);
                        var textInput = textContainer.Children.OfType<TextBox>().FirstOrDefault();

                        // Add to page.
                        sectionContent.Children.Add(textContainer);
                        controlItem.Control = textInput;
                        App.Logger.Info($"Added {textInput.Name} ({controlItem.Type.ToLowerInvariant()}) to the page");
                        break;

                    default:
                        throw new Exception($"Entered type {controlItem.Type} is invalid");
                }
            }

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
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Background = new SolidColorBrush(Colors.Transparent),
                Content = sectionContent
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

    private StackPanel CreateTextBox(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        label.Width = (controlItem.Width > 0) ? controlItem.Width : label.Width;

        //----

        var textbox = new TextBox
        {
            Name = Utilities.EncodeId(controlItem.Name),
            Width = 150,
            Text = Settings.GetSetting<string>(controlItem.Name) ?? string.Empty,
            //ToolTip = tooltip
        };
        textbox.Width = (controlItem.Width > 0) ? controlItem.Width : textbox.Width;
        textbox.LostFocus += (_, _) =>
        {
            Settings.SetSetting(textbox.Name, textbox.Text);
            CheckIsDirty(controlItem);
        };

        //----

        var panel = new StackPanel();
        panel.Margin = new Thickness(10, 5, 0, 5);
        panel.Children.Add(label);
        panel.Children.Add(textbox);
        return panel;
    }

    private Grid CreateBackgroundSetter(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        label.Width = 200;
        Grid.SetColumn(label, 0);
        Grid.SetColumnSpan(label, 2);
        Grid.SetRow(label, 0);

        //----

        // Add preview image
        var image = new Image
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            //Margin = new Thickness(0, 10, 0, 0)
        };

        Grid.SetColumn(image, 0);
        Grid.SetColumnSpan(image, 2);
        Grid.SetRow(image, 2);

        var imageSource = Settings.GetSetting<string>(controlItem.Name);
        if (!string.IsNullOrWhiteSpace(imageSource))
            if (Uri.TryCreate(imageSource, UriKind.Absolute, out var path))
                image.Source = new Bitmap(path.ToString());

        //----

        // Add browse button 
        var browse = new Button
        {
            Name = Utilities.EncodeId(controlItem.Name),
            Content = Assets.Resources.ui_browse
        };
        browse.Classes.Add("BackgroundBrowseClear");
        browse.Click += async (_, _) =>
        {
            await Utilities.ShowMessageBox(Assets.Resources.info_background_override);
            var files = await TopLevel.GetTopLevel(browse).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select an image file",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Image Files")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp"],
                        MimeTypes = ["image/*"]
                    }
                ]
            });

            if (files.Count >= 1)
            {
                await using var stream = await files[0].OpenReadAsync();
                image.Source = new Bitmap(stream);
            }

            CheckIsDirty(controlItem);
        };

        Grid.SetColumn(browse, 0);
        Grid.SetRow(browse, 1);

        //----

        // Add reset button 
        var clear = new Button
        {
            Content = Assets.Resources.ui_clear
        };
        clear.Classes.Add("BackgroundBrowseClear");
        clear.Click += (_, _) =>
        {
            image.Source = null;
            Settings.SetSetting(controlItem.Name, "");
        };

        Grid.SetColumn(clear, 1);
        Grid.SetRow(clear, 1);

        //----

        var panel = new Grid();
        panel.Margin = new Thickness(10, lastTop, 0, 0);
        panel.ColumnDefinitions.Add(new ColumnDefinition());
        panel.ColumnDefinitions.Add(new ColumnDefinition());
        panel.RowDefinitions.Add(new RowDefinition());
        panel.RowDefinitions.Add(new RowDefinition());
        panel.RowDefinitions.Add(new RowDefinition());
        panel.Children.Add(label);
        panel.Children.Add(browse);
        panel.Children.Add(clear);
        panel.Children.Add(image);
        return panel;
    }

    private StackPanel CreateCrosshairPicker(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        label.Width = (controlItem.Width > 0) ? controlItem.Width : label.Width;

        //----

        var combobox = new ComboBox
        {
            Name = Utilities.EncodeId(controlItem.Name),
            //ToolTip = tooltip
        };
        combobox.Classes.Add("CrosshairBox");
        combobox.Width = (controlItem.Width > 0) ? controlItem.Width : combobox.Width;
        combobox.SelectionChanged += (sender, _) =>
        {
            var input = sender as ComboBox;
            var value = (combobox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            Settings.SetSetting(input?.Name, value);
            CheckIsDirty(controlItem);
        };

        foreach (var option in Utilities.CrosshairStyles)
        {
            var item = new ComboBoxItem();
            item.Content = option;
            item.Classes.Add("CrosshairBoxItem");
            combobox.Items.Add(item);
        }

        // Set the selected value depending on the what's retrieved from the setting file.
        var index = Utilities.CrosshairStyles.IndexOf(Settings.GetSetting<string>(Utilities.EncodeId(controlItem.Name)));
        combobox.SelectedIndex = (index >= 0) ? index : 0;

        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && controlItem.Label.Contains("Style"))
            combobox.SelectedValue = App.Config.ConfigSettings.UserPrefs.CrosshairStyle;

        //----

        var panel = new StackPanel();
        panel.Margin = new Thickness(10, lastTop, 0, 5);
        panel.Children.Add(label);
        panel.Children.Add(combobox);
        return panel;
    }

    private StackPanel CreateNumberPicker(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        label.Width = (controlItem.Width > 0) ? controlItem.Width : label.Width;

        //----

        var numpicker = new NumericUpDown
        {
            Name = Utilities.EncodeId(controlItem.Name),
            Value = Settings.GetSetting<int>(Utilities.EncodeId(controlItem.Name)),
            Minimum = controlItem.Minimum,
            Maximum = controlItem.Maximum,
            Increment = controlItem.Increment,
            //ToolTip = tooltip
        };
        numpicker.Width = (controlItem.Width > 0) ? controlItem.Width : numpicker.Width;
        numpicker.ValueChanged += (sender, _) =>
        {
            var input = sender as NumericUpDown;
            Settings.SetSetting(input.Name, input.Text);
            CheckIsDirty(controlItem);
        };

        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && controlItem.Label.Contains("Size"))
            numpicker.Value = App.Config.ConfigSettings.UserPrefs.CrosshairSize;

        //----

        var panel = new StackPanel();
        panel.Margin = new Thickness(10, lastTop, 0, 0);
        panel.Children.Add(label);
        panel.Children.Add(numpicker);
        return panel;
    }

    private StackPanel CreateComboBox(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        label.Width = (controlItem.Width > 0) ? controlItem.Width : label.Width;

        //----

        var combobox = new ComboBox
        {
            Name = Utilities.EncodeId(controlItem.Name),
            Width = 150
            //ToolTip = tooltip
        };
        combobox.Width = (controlItem.Width > 0) ? controlItem.Width : combobox.Width;
        combobox.SelectionChanged += (sender, _) =>
        {
            var input = sender as ComboBox;
            Settings.SetSetting(input?.Name, combobox.SelectedIndex.ToString());
            CheckIsDirty(controlItem);
        };

        foreach (var option in controlItem.Options)
        {
            var item = new ComboBoxItem();
            item.Content = option.Label;
            combobox.Items.Add(item);
        }

        // Set the selected value depending on the what's retrieved from the setting file.
        combobox.SelectedIndex = int.Parse(Settings.GetSetting<string>(Utilities.EncodeId(controlItem.Name)));

        //----

        var panel = new StackPanel();
        panel.Margin = new Thickness(10, lastTop, 0, 5);
        panel.Children.Add(label);
        panel.Children.Add(combobox);
        return panel;
    }

    private StackPanel CreateColorPicker(Models.Controls controlItem)
    {
        var label = new Label();
        label.Content = controlItem.Label;
        label.Classes.Add("OptionLabel");
        //label.Width = (controlItem.Width > 0) ? controlItem.Width : label.Width;

        //----

        var colorpicker = new ColorPicker
        {
            Name = Utilities.EncodeId(controlItem.Name),
            //ToolTip = tooltip
        };
        //control.Width = (controlItem.Width > 0) ? controlItem.Width : control.Width;
        colorpicker.ColorChanged += (sender, _) =>
        {
            var input = sender as ColorPicker;
            Settings.SetSetting(input?.Name, Utilities.ConvertToRgba(input?.Color.ToString()));
        };

        // Attempt to bind the color from the settings.
        try
        {
            if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && controlItem.Label.Contains("Crosshair"))
                colorpicker.Color = Utilities.ConvertToColor(App.Config.ConfigSettings.UserPrefs.CrosshairColor);
            else
                colorpicker.Color = Settings.GetSetting<Color>(Utilities.EncodeId(controlItem.Name));
        }
        catch
        {
            colorpicker.Color = Color.FromArgb(255, 0, 255, 0);
        }

        //----

        var panel = new StackPanel();
        panel.Margin = new Thickness(10, lastTop, 0, 10);
        panel.Children.Add(label);
        panel.Children.Add(colorpicker);
        return panel;
    }

    private Control CreateCheckbox(Models.Controls controlItem)
    {
        var checkbox = new CheckBox
        {
            Name = Utilities.EncodeId(controlItem.Name),
            Content = controlItem.Label,
            Margin = new Thickness(10, lastTop, 0, 0),
            IsChecked = Settings.GetSetting<bool>(Utilities.EncodeId(controlItem.Name)),
            //ToolTip = tooltip
        };
        checkbox.Width = (controlItem.Width > 0) ? controlItem.Width : checkbox.Width;
        checkbox.IsCheckedChanged += (sender, _) =>
        {
            var input = sender as CheckBox;
            Settings.SetSetting(input?.Name, checkbox.IsChecked?.ToString());
            CheckIsDirty(controlItem);
        };

        // Persist the crosshair selection.
        if (App.Config.ConfigSettings.UserPrefs.CrosshairPersistence && controlItem.Label.Contains("Toggle Crosshair"))
            checkbox.IsChecked = App.Config.ConfigSettings.UserPrefs.CrosshairEnabled;

        return checkbox;
    }

    private Button CreateResetButton(string? section)
    {
        var button = new Button
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Content = "\u0157",
            Opacity = 0.4
        };
        button.Classes.Add("PreviewButton");
        button.Click += (_, _) =>
        {
            if (!App.Config.ConfigSettings.UserPrefs.SelectedHUD.Equals(Name)) return;
            ResetSection(section);
            Settings.SaveSettings();
            if (Utilities.CheckHudInstallation(this)) ApplyCustomizations();
            DirtyControls.Clear();
        };

        Grid.SetColumn(button, 1);

        return button;
    }

    private Button CreatePreviewButton(string url)
    {
        // Create the preview modal
        var image = new Image();
        image.Classes.Add("PreviewImage");
        image.Source = Utilities.LoadImage(url);

        var border = new Border
        {
            Background = Brushes.Black,
            Padding = new Thickness(2),
            Child = image
        };

        var button = new Button();
        button.Classes.Add("PreviewButton");
        button.Click += (_, _) => DialogHost.Show(border, "PreviewModal");
        return button;
    }

    /// <summary>
    /// Checks whether a control change requires a game restart.
    /// </summary>
    private void CheckIsDirty(Models.Controls control)
    {
        if (control.Restart && !string.Equals(control.Value, Settings.GetSetting(control.Name).Value) && !DirtyControls.Contains(control.Label))
            DirtyControls.Add(control.Label);
        else
            DirtyControls.Remove(control.Label);
    }
}