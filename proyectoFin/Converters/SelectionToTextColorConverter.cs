using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace proyectoFin.Converters;

public class SelectionToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isSelected = (bool)value;
        return isSelected ? Color.FromArgb("#FFF5E1") : Color.FromArgb("#5C4033");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
