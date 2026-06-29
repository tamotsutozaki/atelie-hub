using System.ComponentModel;

namespace AtelieHub.Core.Enums;

public enum TipoLancamento
{
    [Description("Entrada")] Entrada = 0,
    [Description("Saída")] Saida = 1,
}
