using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace proyectoFin.Converters;

public class SelectionToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isSelected = (bool)value;
        return isSelected ? Color.FromArgb("#5C4033") : Color.FromArgb("#FDF1EB");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
