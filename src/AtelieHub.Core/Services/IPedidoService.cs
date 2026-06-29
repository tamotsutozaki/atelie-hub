using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;

namespace AtelieHub.Core.Services;

public interface IPedidoService
{
    /// <summary>Lista pedidos (com o Cliente carregado), filtrando por texto e/ou status.</summary>
    Task<IReadOnlyList<Pedido>> ListarAsync(string? busca = null, StatusPedido? status = null, CancellationToken ct = default);

    Task<Pedido?> ObterAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cria (gera número sequencial) ou atualiza um pedido (merge por Id).</summary>
    Task<Pedido> SalvarAsync(Pedido pedido, CancellationToken ct = default);

    /// <summary>Atualiza apenas o status de produção do pedido.</summary>
    Task DefinirStatusAsync(Guid id, StatusPedido status, CancellationToken ct = default);
}
