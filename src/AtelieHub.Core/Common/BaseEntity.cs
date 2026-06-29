namespace AtelieHub.Core.Common;

/// <summary>
/// Base de todas as entidades. Id em Guid (facilita multi-tenant/sync futuro com Supabase).
/// Os timestamps são preenchidos automaticamente pelo AppDbContext.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
