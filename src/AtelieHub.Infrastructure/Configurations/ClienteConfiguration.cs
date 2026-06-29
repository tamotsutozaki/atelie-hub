using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Telefone).HasMaxLength(40);
        builder.Property(c => c.Email).HasMaxLength(160);
        builder.Property(c => c.Instagram).HasMaxLength(80);
        builder.Property(c => c.Origem).HasMaxLength(120);
        builder.Property(c => c.Observacoes).HasMaxLength(2000);
        builder.Property(c => c.Cep).HasMaxLength(12);
        builder.Property(c => c.Logradouro).HasMaxLength(300);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(120);
        builder.Property(c => c.Bairro).HasMaxLength(120);
        builder.Property(c => c.Cidade).HasMaxLength(120);
        builder.Property(c => c.Estado).HasMaxLength(60);
        builder.Property(c => c.Ativo).HasDefaultValue(true);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.EmpresaId, c.Nome });
    }
}
