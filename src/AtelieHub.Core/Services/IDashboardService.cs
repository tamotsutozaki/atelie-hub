namespace AtelieHub.Core.Services;

/// <summary>Uma linha da lista de um indicador do painel (o que o número representa).</summary>
public record DashboardItem(string Texto, string Detalhe);

/// <summary>Números consolidados para o painel inicial, cada um acompanhado da lista de itens a que se refere.</summary>
public record DashboardResumo(
    int PedidosEmAberto,
    int AEntregar,
    int PendenciasFinanceiras,
    int EstoqueBaixo,
    int MarketingPendente,
    decimal SaldoCaixa,
    IReadOnlyList<DashboardItem> PedidosEmAbertoItens,
    IReadOnlyList<DashboardItem> AEntregarItens,
    IReadOnlyList<DashboardItem> PendenciasFinanceirasItens,
    IReadOnlyList<DashboardItem> EstoqueBaixoItens,
    IReadOnlyList<DashboardItem> MarketingPendenteItens);

public interface IDashboardService
{
    Task<DashboardResumo> ObterResumoAsync(CancellationToken ct = default);
}
