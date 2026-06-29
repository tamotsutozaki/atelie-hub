using AtelieHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieHub.Infrastructure.Configurations;

public class TarefaMarketingConfiguration : IEntityTypeConfiguration<TarefaMarketing>
{
    public void Configure(EntityTypeBuilder<TarefaMarketing> builder)
    {
        builder.ToTable("tarefas_marketing");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Descricao).HasMaxLength(2000);
        builder.Property(t => t.DataPrevista).HasColumnType("timestamp without time zone");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.EmpresaId, t.DataPrevista });
    }
}
