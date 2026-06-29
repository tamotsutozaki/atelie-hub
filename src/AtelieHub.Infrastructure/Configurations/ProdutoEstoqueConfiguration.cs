using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class ProdutoEstoqueConfiguration : IEntityTypeConfiguration<ProdutoEstoque>
{
    public void Configure(EntityTypeBuilder<ProdutoEstoque> builder)
    {
        builder.ToTable("produtos_estoque");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Unidade).HasMaxLength(20);
        builder.Property(p => p.Observacoes).HasMaxLength(2000);
        builder.Property(p => p.Quantidade).HasPrecision(18, 3);
        builder.Property(p => p.EstoqueMinimo).HasPrecision(18, 3);
        builder.Property(p => p.Custo).HasPrecision(18, 2);
        builder.Property(p => p.Ativo).HasDefaultValue(true);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.EmpresaId, p.Nome });
    }
}
