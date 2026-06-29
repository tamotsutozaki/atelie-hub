using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EmpresaService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> ExisteEmpresaAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Empresas.AnyAsync(ct);
    }

    public async Task<Empresa?> ObterAtualAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Empresas.OrderBy(e => e.CriadoEm).FirstOrDefaultAsync(ct);
    }

    public async Task<Empresa> SalvarAsync(Empresa empresa, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var existe = await db.Empresas.AnyAsync(e => e.Id == empresa.Id, ct);
        if (existe)
        {
            db.Empresas.Update(empresa);
        }
        else
        {
            db.Empresas.Add(empresa);
        }

        await db.SaveChangesAsync(ct);
        return empresa;
    }
}
