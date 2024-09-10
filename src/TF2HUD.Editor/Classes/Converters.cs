using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HUDEditor.Properties;

namespace HUDEditor.Classes
{
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
                string s => !string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Collapsed,
                _ => Visibility.Hidden
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
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
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
            return value is null ? Visibility.Visible : Visibility.Collapsed;
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
                ImageSource = new BitmapImage(new Uri(Settings.Default.app_default_bg, UriKind.RelativeOrAbsolute))
            };

            var selection = (HUD)value;
            selection.Background ??= Settings.Default.app_default_bg;
            if (selection.Background.StartsWith("http") || selection.Background.StartsWith("file"))
            {
                return new ImageBrush
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = selection.Opacity,
                    ImageSource = new BitmapImage(new Uri(selection.Background ??= Settings.Default.app_default_bg, UriKind.RelativeOrAbsolute))
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
            return (bool)value ? Brushes.SkyBlue : Brushes.White;
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
                MainWindow.Logger.Info($"User selected: {hud.Name}");
                if (Directory.Exists($"{Settings.Default.hud_directory}\\{hud.Name}"))
                {
                    MainWindow.Logger.Info($"{hud.Name} is installed");
                    return Utilities.GetLocalizedString("ui_reinstall") ?? "Reinstall";
                }

                MainWindow.Logger.Warn($"{hud.Name} is not installed");
                return Utilities.GetLocalizedString("ui_install") ?? "Install";
            }

            MainWindow.Logger.Warn("User selected HUD is null. Returning to the main menu");
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
            return value is not null && ((IEnumerable<object>)value).Count() > 1 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}