namespace AtelieHub.Core.Common;

/// <summary>
/// Base de todas as entidades. Id em Guid (facilita multi-tenant/sync futuro com Supabase).
/// Os timestamps são preenchidos automaticamente pelo AppDbContext.
/// </summary>
public abstract class BaseEntity
{
    // UUID v7 (ordenado por tempo): melhor localidade de índice na PK e ordem natural de criação.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
