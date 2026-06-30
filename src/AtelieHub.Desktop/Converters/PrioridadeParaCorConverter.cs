using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AtelieHub.Core.Enums;

namespace AtelieHub.Desktop.Converters;

/// <summary>Converte a prioridade de um pedido numa cor (bolinha do cartão no Kanban).</summary>
public class PrioridadeParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Baixa = Congelar(Color.FromRgb(0xD8, 0xC7, 0xAE));  // areia
    private static readonly SolidColorBrush Normal = Congelar(Color.FromRgb(0x6F, 0x4E, 0x37)); // marrom
    private static readonly SolidColorBrush Alta = Congelar(Color.FromRgb(0xB2, 0x3B, 0x3B));   // vermelho

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is PrioridadePedido prioridade
            ? prioridade switch
            {
                PrioridadePedido.Alta => Alta,
                PrioridadePedido.Baixa => Baixa,
                _ => Normal,
            }
            : Normal;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;

    private static SolidColorBrush Congelar(Color cor)
    {
        var brush = new SolidColorBrush(cor);
        brush.Freeze();
        return brush;
    }
}
