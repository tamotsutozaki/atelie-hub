using AtelieHub.Core.Entities;

namespace AtelieHub.Core.Services;

public interface IClienteService
{
    /// <summary>Lista clientes, opcionalmente filtrando por texto (nome/telefone/email/instagram) e incluindo inativos.</summary>
    Task<IReadOnlyList<Cliente>> ListarAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default);

    Task<Cliente?> ObterAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cria ou atualiza um cliente (merge por Id, preservando EmpresaId e CriadoEm).</summary>
    Task<Cliente> SalvarAsync(Cliente cliente, CancellationToken ct = default);

    /// <summary>Inativa (false) ou reativa (true) um cliente, sem apagar o registro.</summary>
    Task DefinirAtivoAsync(Guid id, bool ativo, CancellationToken ct = default);

    /// <summary>
    /// Exclui o cliente definitivamente. Lança <see cref="InvalidOperationException"/> se ele
    /// tiver pedidos vinculados (o vínculo é Restrict) — nesse caso a opção é inativar.
    /// </summary>
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}
