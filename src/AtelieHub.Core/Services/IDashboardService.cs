namespace AtelieHub.Core.Services;

/// <summary>Números consolidados para o painel inicial.</summary>
public record DashboardResumo(
    int PedidosEmAberto,
    int AEntregar,
    int PendenciasFinanceiras,
    int EstoqueBaixo,
    int MarketingPendente,
    decimal SaldoCaixa);

public interface IDashboardService
{
    Task<DashboardResumo> ObterResumoAsync(CancellationToken ct = default);
}
