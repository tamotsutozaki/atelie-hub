using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class PedidoService : IPedidoService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PedidoService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Pedido>> ListarAsync(string? busca = null, StatusPedido? status = null, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{EscaparCuringa(busca.Trim())}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Titulo, termo, "\\") ||
                (p.Cliente != null && EF.Functions.ILike(p.Cliente.Nome, termo, "\\")));
        }

        // Urgentes primeiro; depois por prazo (sem prazo vai para o fim); depois prioridade alta.
        return await query
            .OrderByDescending(p => p.Urgente)
            .ThenBy(p => p.PrazoEntrega)
            .ThenByDescending(p => p.Prioridade)
            .ThenByDescending(p => p.Numero)
            .ToListAsync(ct);
    }

    public async Task<Pedido?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Pedidos.AsNoTracking().Include(p => p.Cliente).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Pedido> SalvarAsync(Pedido pedido, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existente = await db.Pedidos.FirstOrDefaultAsync(p => p.Id == pedido.Id, ct);
        if (existente is null)
        {
            if (pedido.EmpresaId == Guid.Empty)
            {
                pedido.EmpresaId = await db.Empresas.Select(e => e.Id).FirstAsync(ct);
            }

            // Próximo número sequencial dentro da empresa.
            var ultimo = await db.Pedidos
                .Where(p => p.EmpresaId == pedido.EmpresaId)
                .MaxAsync(p => (int?)p.Numero, ct) ?? 0;
            pedido.Numero = ultimo + 1;

            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync(ct);
            return pedido;
        }

        existente.ClienteId = pedido.ClienteId;
        existente.Titulo = pedido.Titulo;
        existente.Descricao = pedido.Descricao;
        existente.Tipo = pedido.Tipo;
        existente.Status = pedido.Status;
        existente.Prioridade = pedido.Prioridade;
        existente.Urgente = pedido.Urgente;
        existente.DataPedido = pedido.DataPedido;
        existente.PrazoEntrega = pedido.PrazoEntrega;
        existente.ValorTotal = pedido.ValorTotal;
        existente.StatusPagamento = pedido.StatusPagamento;
        existente.Observacoes = pedido.Observacoes;
        existente.Transportadora = pedido.Transportadora;
        existente.CodigoRastreio = pedido.CodigoRastreio;
        existente.DataPostagem = pedido.DataPostagem;

        await db.SaveChangesAsync(ct);
        return existente;
    }

    public async Task DefinirStatusAsync(Guid id, StatusPedido status, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var pedido = await db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pedido is null)
        {
            return;
        }

        pedido.Status = status;
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var pedido = await db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pedido is null)
        {
            return;
        }

        // Lançamentos financeiros têm PedidoId apenas informativo (sem FK): permanecem após a
        // exclusão — apagar o pedido não deve sumir com o dinheiro já registrado.
        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Pedido>> ListarEmProducaoVencendoAsync(int dias, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Data de negócio (Kind local): PrazoEntrega é 'timestamp without time zone' e o Npgsql
        // rejeita comparação com DateTime de Kind=Utc. DateTime.Today é consistente com a gravação.
        var limite = DateTime.Today.AddDays(dias);

        // Em produção: vencendo na janela OU sem prazo (peças que ela está fazendo agora também
        // entram no foco). Sem prazo vai para o fim (Postgres ordena NULLS LAST no ASC).
        return await db.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.Status == StatusPedido.EmProducao
                        && (p.PrazoEntrega == null || p.PrazoEntrega <= limite))
            .OrderByDescending(p => p.Urgente)
            .ThenBy(p => p.PrazoEntrega)
            .ToListAsync(ct);
    }

    private static string EscaparCuringa(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
