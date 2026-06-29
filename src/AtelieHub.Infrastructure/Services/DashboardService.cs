using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DashboardService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<DashboardResumo> ObterResumoAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var pedidosEmAberto = await db.Pedidos.CountAsync(
            p => p.Status != StatusPedido.Concluido && p.Status != StatusPedido.Cancelado, ct);

        var aEntregar = await db.Pedidos.CountAsync(p => p.Status == StatusPedido.Pronto, ct);

        var pendenciasFinanceiras = await db.Pedidos.CountAsync(
            p => p.StatusPagamento != StatusPagamento.Pago && p.Status != StatusPedido.Cancelado, ct);

        var estoqueBaixo = await db.ProdutosEstoque.CountAsync(ProdutoEstoque.EstoqueBaixoPredicado, ct);

        var marketingPendente = await db.TarefasMarketing.CountAsync(t => !t.Concluido, ct);

        var entradas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Entrada)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        var saidas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Saida)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        return new DashboardResumo(
            pedidosEmAberto,
            aEntregar,
            pendenciasFinanceiras,
            estoqueBaixo,
            marketingPendente,
            entradas - saidas);
    }
}
