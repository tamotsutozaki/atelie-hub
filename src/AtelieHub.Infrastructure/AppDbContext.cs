using AtelieHub.Core.Common;
using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        AplicarTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AplicarTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Preenche CriadoEm/AtualizadoEm automaticamente (sempre em UTC, exigência do Npgsql para timestamptz).</summary>
    private void AplicarTimestamps()
    {
        var agora = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CriadoEm = agora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.AtualizadoEm = agora;
                // CriadoEm é imutável: nunca sobrescrever em update (protege contra Update()
                // de entidade desanexada, que marcaria todas as colunas como Modified).
                entry.Property(e => e.CriadoEm).IsModified = false;
            }
        }
    }
}
