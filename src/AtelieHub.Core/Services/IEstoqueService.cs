using AtelieHub.Core.Entities;

namespace AtelieHub.Core.Services;

public interface IEstoqueService
{
    Task<IReadOnlyList<ProdutoEstoque>> ListarAsync(string? busca = null, bool somenteBaixo = false, bool incluirInativos = false, CancellationToken ct = default);

    Task<ProdutoEstoque> SalvarAsync(ProdutoEstoque produto, CancellationToken ct = default);

    Task DefinirAtivoAsync(Guid id, bool ativo, CancellationToken ct = default);

    /// <summary>Quantidade de itens ativos abaixo (ou igual) do estoque mínimo — usado no painel.</summary>
    Task<int> ContarBaixoAsync(CancellationToken ct = default);
}
