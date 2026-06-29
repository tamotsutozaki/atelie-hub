using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descricao).HasMaxLength(4000);
        builder.Property(p => p.Observacoes).HasMaxLength(4000);
        builder.Property(p => p.Transportadora).HasMaxLength(120);
        builder.Property(p => p.CodigoRastreio).HasMaxLength(80);
        builder.Property(p => p.ValorTotal).HasPrecision(18, 2);

        // Datas de negócio: timestamp sem fuso (vêm do DatePicker como hora local/Unspecified).
        builder.Property(p => p.DataPedido).HasColumnType("timestamp without time zone");
        builder.Property(p => p.PrazoEntrega).HasColumnType("timestamp without time zone");
        builder.Property(p => p.DataPostagem).HasColumnType("timestamp without time zone");

        builder.HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.EmpresaId, p.Status });
        builder.HasIndex(p => new { p.EmpresaId, p.Numero }).IsUnique();
    }
}
