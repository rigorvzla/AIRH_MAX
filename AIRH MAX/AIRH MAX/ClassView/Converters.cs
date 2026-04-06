using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace AIRH_MAX.ClassView
{
    // Tus converters existentes...
    public class MessageMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HorizontalAlignment alignment)
            {
                return alignment == HorizontalAlignment.Right
                    ? new Thickness(50, 2, 0, 2)
                    : alignment == HorizontalAlignment.Left
                        ? new Thickness(0, 2, 50, 2)
                        : new Thickness(0, 2, 0, 2);
            }
            return new Thickness(0, 2, 0, 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isSystem && isSystem ? 2 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ============ NUEVOS CONVERTERS AGREGADOS ============

    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter?.ToString() == "text")
            {
                if (value is bool isSystem && isSystem)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            }

            if (value is string status)
            {
                return status switch
                {
                    "Online" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                    "Conectando..." => new SolidColorBrush(Colors.Orange),
                    "Reconectando..." => new SolidColorBrush(Colors.Orange),
                    "Desconectado" => new SolidColorBrush(Colors.Red),
                    "Error de conexión" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isSystem && isSystem) ? FontStyles.Italic : FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isSystem && isSystem) ? 10 : 12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}