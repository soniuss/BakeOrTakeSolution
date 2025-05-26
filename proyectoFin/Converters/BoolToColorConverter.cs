// BoolToColorConverter.cs (DESPUÉS)
using System.Globalization;
using Microsoft.Maui.Controls;

namespace proyectoFin.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParams)
            {
                var colors = colorParams.Split('|');
                if (colors.Length == 2)
                {
                    string trueColorHex = colors[0];
                    string falseColorHex = colors[1];

                    try
                    {
                        // Intentamos parsear la cadena hexadecimal a un objeto Color
                        Color trueColor = Color.FromArgb(trueColorHex);
                        Color falseColor = Color.FromArgb(falseColorHex);

                        return boolValue ? trueColor : falseColor;
                    }
                    catch (Exception ex)
                    {
                        // Manejo de errores si la cadena no es un formato de color válido
                        System.Diagnostics.Debug.WriteLine($"Error al convertir color hexadecimal: {ex.Message}");
                        return Colors.Transparent; // Devuelve un color por defecto en caso de error
                    }
                }
            }
            return Colors.Transparent; // Valor por defecto si los parámetros no son válidos
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}