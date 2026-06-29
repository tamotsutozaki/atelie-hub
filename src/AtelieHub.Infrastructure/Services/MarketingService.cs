using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class MarketingService : IMarketingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public MarketingService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<TarefaMarketing>> ListarAsync(bool incluirConcluidas = false, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.TarefasMarketing.AsNoTracking().AsQueryable();

        if (!incluirConcluidas)
        {
            query = query.Where(t => !t.Concluido);
        }

        // Pendentes primeiro; depois por data prevista (sem data vai para o fim).
        return await query
            .OrderBy(t => t.Concluido)
            .ThenBy(t => t.DataPrevista)
            .ToListAsync(ct);
    }

    public async Task<TarefaMarketing> SalvarAsync(TarefaMarketing tarefa, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existente = await db.TarefasMarketing.FirstOrDefaultAsync(t => t.Id == tarefa.Id, ct);
        if (existente is null)
        {
            if (tarefa.EmpresaId == Guid.Empty)
            {
                tarefa.EmpresaId = await db.Empresas.Select(e => e.Id).FirstAsync(ct);
            }

            db.TarefasMarketing.Add(tarefa);
            await db.SaveChangesAsync(ct);
            return tarefa;
        }

        existente.Titulo = tarefa.Titulo;
        existente.Descricao = tarefa.Descricao;
        existente.Rede = tarefa.Rede;
        existente.Tipo = tarefa.Tipo;
        existente.DataPrevista = tarefa.DataPrevista;
        existente.Concluido = tarefa.Concluido;

        await db.SaveChangesAsync(ct);
        return existente;
    }

    public async Task DefinirConcluidoAsync(Guid id, bool concluido, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var tarefa = await db.TarefasMarketing.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tarefa is null)
        {
            return;
        }

        tarefa.Concluido = concluido;
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var tarefa = await db.TarefasMarketing.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tarefa is null)
        {
            return;
        }

        db.TarefasMarketing.Remove(tarefa);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> ContarPendentesAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.TarefasMarketing.CountAsync(t => !t.Concluido, ct);
    }
}
