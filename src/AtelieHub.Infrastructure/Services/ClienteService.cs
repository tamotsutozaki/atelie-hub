using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ClienteService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.Clientes.AsNoTracking().AsQueryable();

        if (!incluirInativos)
        {
            query = query.Where(c => c.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{EscaparCuringa(busca.Trim())}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Nome, termo, "\\") ||
                (c.Telefone != null && EF.Functions.ILike(c.Telefone, termo, "\\")) ||
                (c.Email != null && EF.Functions.ILike(c.Email, termo, "\\")) ||
                (c.Instagram != null && EF.Functions.ILike(c.Instagram, termo, "\\")));
        }

        return await query.OrderBy(c => c.Nome).ToListAsync(ct);
    }

    public async Task<Cliente?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Cliente> SalvarAsync(Cliente cliente, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id, ct);
        if (existente is null)
        {
            if (cliente.EmpresaId == Guid.Empty)
            {
                // Resolve a empresa atual (há uma por instalação). É o ponto de costura para multi-tenant futuro.
                cliente.EmpresaId = await db.Empresas.Select(e => e.Id).FirstAsync(ct);
            }

            db.Clientes.Add(cliente);
            await db.SaveChangesAsync(ct);
            return cliente;
        }

        // Merge dos campos editáveis, preservando Id, EmpresaId e CriadoEm.
        existente.Nome = cliente.Nome;
        existente.Telefone = cliente.Telefone;
        existente.Email = cliente.Email;
        existente.Instagram = cliente.Instagram;
        existente.Origem = cliente.Origem;
        existente.Observacoes = cliente.Observacoes;
        existente.Cep = cliente.Cep;
        existente.Logradouro = cliente.Logradouro;
        existente.Numero = cliente.Numero;
        existente.Complemento = cliente.Complemento;
        existente.Bairro = cliente.Bairro;
        existente.Cidade = cliente.Cidade;
        existente.Estado = cliente.Estado;
        existente.Ativo = cliente.Ativo;

        await db.SaveChangesAsync(ct);
        return existente;
    }

    /// <summary>Escapa os curingas do LIKE (\ % _) para que a busca trate o texto digitado como literal.</summary>
    private static string EscaparCuringa(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public async Task DefinirAtivoAsync(Guid id, bool ativo, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cliente is null)
        {
            return;
        }

        cliente.Ativo = ativo;
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cliente is null)
        {
            return;
        }

        // O vínculo Pedido→Cliente é Restrict: bloquear aqui dá uma mensagem clara em vez do
        // erro cru de chave estrangeira vindo do banco.
        var qtdPedidos = await db.Pedidos.CountAsync(p => p.ClienteId == id, ct);
        if (qtdPedidos > 0)
        {
            throw new InvalidOperationException(
                $"Este cliente tem {qtdPedidos} pedido(s) vinculado(s) e não pode ser excluído.\n\n" +
                "Exclua os pedidos primeiro ou use \"Inativar\" para apenas ocultá-lo da lista.");
        }

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync(ct);
    }
}
