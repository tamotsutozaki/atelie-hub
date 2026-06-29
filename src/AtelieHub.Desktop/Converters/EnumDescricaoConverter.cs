using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AtelieHub.Desktop.Converters;

/// <summary>Converte um valor de enum no texto do seu [Description] (rótulo amigável em pt-BR).</summary>
public class EnumDescricaoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var tipo = value.GetType();
        if (!tipo.IsEnum)
        {
            return value.ToString() ?? string.Empty;
        }

        var nome = value.ToString()!;
        var membro = tipo.GetMember(nome).FirstOrDefault();
        var descricao = membro?.GetCustomAttribute<DescriptionAttribute>();
        return descricao?.Description ?? nome;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
