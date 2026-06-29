using AtelieHub.Core.Entities;

namespace AtelieHub.Core.Services;

public interface IMarketingService
{
    Task<IReadOnlyList<TarefaMarketing>> ListarAsync(bool incluirConcluidas = false, CancellationToken ct = default);

    Task<TarefaMarketing> SalvarAsync(TarefaMarketing tarefa, CancellationToken ct = default);

    Task DefinirConcluidoAsync(Guid id, bool concluido, CancellationToken ct = default);

    Task RemoverAsync(Guid id, CancellationToken ct = default);

    /// <summary>Quantidade de lembretes ainda não concluídos — usado no painel.</summary>
    Task<int> ContarPendentesAsync(CancellationToken ct = default);
}
