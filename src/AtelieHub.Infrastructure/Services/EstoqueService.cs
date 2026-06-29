using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EstoqueService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ProdutoEstoque>> ListarAsync(string? busca = null, bool somenteBaixo = false, bool incluirInativos = false, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.ProdutosEstoque.AsNoTracking().AsQueryable();

        if (!incluirInativos)
        {
            query = query.Where(p => p.Ativo);
        }

        if (somenteBaixo)
        {
            query = query.Where(ProdutoEstoque.EstoqueBaixoPredicado);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{EscaparCuringa(busca.Trim())}%";
            query = query.Where(p => EF.Functions.ILike(p.Nome, termo, "\\"));
        }

        return await query.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task<ProdutoEstoque> SalvarAsync(ProdutoEstoque produto, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existente = await db.ProdutosEstoque.FirstOrDefaultAsync(p => p.Id == produto.Id, ct);
        if (existente is null)
        {
            if (produto.EmpresaId == Guid.Empty)
            {
                produto.EmpresaId = await db.Empresas.Select(e => e.Id).FirstAsync(ct);
            }

            db.ProdutosEstoque.Add(produto);
            await db.SaveChangesAsync(ct);
            return produto;
        }

        existente.Nome = produto.Nome;
        existente.Unidade = produto.Unidade;
        existente.Quantidade = produto.Quantidade;
        existente.EstoqueMinimo = produto.EstoqueMinimo;
        existente.Custo = produto.Custo;
        existente.Observacoes = produto.Observacoes;
        existente.Ativo = produto.Ativo;

        await db.SaveChangesAsync(ct);
        return existente;
    }

    public async Task DefinirAtivoAsync(Guid id, bool ativo, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var produto = await db.ProdutosEstoque.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produto is null)
        {
            return;
        }

        produto.Ativo = ativo;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> ContarBaixoAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.ProdutosEstoque.CountAsync(ProdutoEstoque.EstoqueBaixoPredicado, ct);
    }

    private static string EscaparCuringa(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
