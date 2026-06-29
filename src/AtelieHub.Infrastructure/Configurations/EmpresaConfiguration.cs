using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(160);
        builder.Property(e => e.NomeResponsavel).HasMaxLength(160);
        builder.Property(e => e.Documento).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(160);
        builder.Property(e => e.Telefone).HasMaxLength(40);
        builder.Property(e => e.Instagram).HasMaxLength(80);
        builder.Property(e => e.LogoPath).HasMaxLength(500);
        builder.Property(e => e.Cep).HasMaxLength(12);
        builder.Property(e => e.Endereco).HasMaxLength(300);
        builder.Property(e => e.Cidade).HasMaxLength(120);
        builder.Property(e => e.Estado).HasMaxLength(60);
    }
}
