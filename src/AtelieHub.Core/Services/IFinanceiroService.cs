using AtelieHub.Core.Entities;

namespace AtelieHub.Core.Services;

public record ResumoFinanceiro(decimal Entradas, decimal Saidas, decimal Saldo);

public interface IFinanceiroService
{
    Task<IReadOnlyList<LancamentoFinanceiro>> ListarAsync(CancellationToken ct = default);

    Task<LancamentoFinanceiro> SalvarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default);

    Task RemoverAsync(Guid id, CancellationToken ct = default);

    Task<ResumoFinanceiro> ResumoAsync(CancellationToken ct = default);
}
