using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public FinanceiroService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<LancamentoFinanceiro>> ListarAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.LancamentosFinanceiros
            .AsNoTracking()
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.CriadoEm)
            .ToListAsync(ct);
    }

    public async Task<LancamentoFinanceiro> SalvarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existente = await db.LancamentosFinanceiros.FirstOrDefaultAsync(l => l.Id == lancamento.Id, ct);
        if (existente is null)
        {
            if (lancamento.EmpresaId == Guid.Empty)
            {
                lancamento.EmpresaId = await db.Empresas.Select(e => e.Id).FirstAsync(ct);
            }

            db.LancamentosFinanceiros.Add(lancamento);
            await db.SaveChangesAsync(ct);
            return lancamento;
        }

        existente.Tipo = lancamento.Tipo;
        existente.Descricao = lancamento.Descricao;
        existente.Categoria = lancamento.Categoria;
        existente.Valor = lancamento.Valor;
        existente.Data = lancamento.Data;
        existente.PedidoId = lancamento.PedidoId;

        await db.SaveChangesAsync(ct);
        return existente;
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var lancamento = await db.LancamentosFinanceiros.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lancamento is null)
        {
            return;
        }

        db.LancamentosFinanceiros.Remove(lancamento);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ResumoFinanceiro> ResumoAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var entradas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Entrada)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        var saidas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Saida)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        return new ResumoFinanceiro(entradas, saidas, entradas - saidas);
    }
}
