using AtelieHub.Core.Common;
using AtelieHub.Core.Enums;

namespace AtelieHub.Core.Entities;

/// <summary>
/// Pedido / trabalho de arte. Coração da operação: status de produção, prioridade,
/// prazo, urgência, parceria, situação de pagamento e dados de envio.
/// </summary>
public class Pedido : BaseEntity
{
    public Guid EmpresaId { get; set; }

    public Guid ClienteId { get; set; }

    /// <summary>Número sequencial amigável por empresa (#1, #2, ...).</summary>
    public int Numero { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public TipoPedido Tipo { get; set; } = TipoPedido.Venda;

    public StatusPedido Status { get; set; } = StatusPedido.Orcamento;

    public PrioridadePedido Prioridade { get; set; } = PrioridadePedido.Normal;

    /// <summary>Pedido de urgência (fora do prazo mínimo).</summary>
    public bool Urgente { get; set; }

    public DateTime DataPedido { get; set; } = DateTime.Today;

    public DateTime? PrazoEntrega { get; set; }

    public decimal ValorTotal { get; set; }

    public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.Pendente;

    public string? Observacoes { get; set; }

    // ===== Envio =====
    public string? Transportadora { get; set; }
    public string? CodigoRastreio { get; set; }
    public DateTime? DataPostagem { get; set; }

    public Cliente? Cliente { get; set; }
}
