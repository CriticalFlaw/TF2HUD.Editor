using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HUDEdit.Classes;

public class NullCheckConverter : IValueConverter
{
    /// <summary>
    /// Returns true if the provided value is not null or empty.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            bool => value,
            _ => true
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotNullCheckConverter : IValueConverter
{
    /// <summary>
    /// Returns true if the provided value is null.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LinkCheckConverterVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            _ => false
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullCheckConverterVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotNullCheckConverterVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PageBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return new ImageBrush
        {
            Stretch = Stretch.UniformToFill,
            Source = Utilities.LoadImage(new Uri(App.Config.ConfigSettings.UserPrefs.BackgroundImage, UriKind.RelativeOrAbsolute).ToString())
        };

        var selection = (HUD)value;
        selection.Background ??= App.Config.ConfigSettings.UserPrefs.BackgroundImage;
        if (selection.Background.StartsWith("http") || selection.Background.StartsWith("file"))
        {
            var image = Utilities.LoadImage(selection.Background);
            return new ImageBrush
            {
                Stretch = Stretch.UniformToFill,
                Opacity = selection.Opacity,
                Source = image
            };
        }

        // The background is an RGBA color code, change it to ARGB and set it as the background.
        return Utilities.ConvertToColorBrush(selection.Background);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DisplayUniqueHudsOnlyForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "\u05AE" : "\u05AF";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BtnInstallContentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hud = (HUD)value;
        if (hud is not null)
        {
            App.Logger.Info($"User selected: {hud.Name}");
            if (Directory.Exists($"{App.Config.ConfigSettings.UserPrefs.HUDDirectory}\\{hud.Name}"))
            {
                App.Logger.Info($"{hud.Name} is installed");
                return Utilities.GetLocalizedString("ui_reinstall") ?? "Reinstall";
            }

            App.Logger.Warn($"{hud.Name} is not installed");
            return Utilities.GetLocalizedString("ui_install") ?? "Install";
        }

        App.Logger.Warn("User selected HUD is null. Returning to the main menu");
        return Utilities.GetLocalizedString("ui_install") ?? "Install";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PresetSelectedStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Application.Current.Resources[$"HudButton{((bool)value ? "Selected" : "")}"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ComboBoxItemsConverterVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null && ((IEnumerable<object>)value).Count() > 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}