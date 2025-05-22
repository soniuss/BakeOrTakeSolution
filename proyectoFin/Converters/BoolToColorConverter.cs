using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoFin.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue && parameter is string colors)
            {
                var colorArray = colors.Split('|');
                if (colorArray.Length == 2)
                {
                    // Asume que los nombres de los colores estan definidos en tus Resources/Styles.xaml
                    // (ej. PrimaryColor, SecondaryColor)
                    var trueColor = (Color)App.Current.Resources[colorArray[0]];
                    var falseColor = (Color)App.Current.Resources[colorArray[1]];
                    return isTrue ? trueColor : falseColor;
                }
            }
            return Colors.Transparent; // Color por defecto si hay un problema
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
