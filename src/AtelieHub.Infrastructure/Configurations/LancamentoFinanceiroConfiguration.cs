using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class LancamentoFinanceiroConfiguration : IEntityTypeConfiguration<LancamentoFinanceiro>
{
    public void Configure(EntityTypeBuilder<LancamentoFinanceiro> builder)
    {
        builder.ToTable("lancamentos_financeiros");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Categoria).HasMaxLength(120);
        builder.Property(l => l.Valor).HasPrecision(18, 2);
        builder.Property(l => l.Data).HasColumnType("timestamp without time zone");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(l => l.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.EmpresaId, l.Data });
    }
}
