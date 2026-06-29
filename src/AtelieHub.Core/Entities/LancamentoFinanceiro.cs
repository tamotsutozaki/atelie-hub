using AtelieHub.Core.Common;
using AtelieHub.Core.Enums;

namespace AtelieHub.Core.Entities;

/// <summary>Movimento de caixa (entrada ou saída). Pode opcionalmente referenciar um pedido.</summary>
public class LancamentoFinanceiro : BaseEntity
{
    public Guid EmpresaId { get; set; }

    /// <summary>Pedido relacionado, se houver (ex.: pagamento recebido de um pedido).</summary>
    public Guid? PedidoId { get; set; }

    public TipoLancamento Tipo { get; set; } = TipoLancamento.Entrada;

    public string Descricao { get; set; } = string.Empty;

    public string? Categoria { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; } = DateTime.Today;
}
