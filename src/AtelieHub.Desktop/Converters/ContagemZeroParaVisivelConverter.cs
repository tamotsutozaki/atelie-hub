using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AtelieHub.Desktop.Converters;

/// <summary>Visível quando a contagem é zero (para mostrar o aviso de lista vazia); caso contrário, oculto.</summary>
public class ContagemZeroParaVisivelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var contagem = value is int n ? n : 0;
        return contagem == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
