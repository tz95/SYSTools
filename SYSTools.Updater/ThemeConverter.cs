using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SYSTools.Updater
{
    public class ThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDarkMode = (bool)value;
            string param = parameter as string;

            switch (param)
            {
                case "Background":
                    return isDarkMode ? Color.FromRgb(32, 32, 32) : Color.FromRgb(255, 255, 255);
                case "LogBackground":
                    return isDarkMode ? Color.FromRgb(45, 45, 45) : Color.FromRgb(240, 240, 240);
                case "Text":
                    return isDarkMode ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);
                default:
                    return Colors.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 