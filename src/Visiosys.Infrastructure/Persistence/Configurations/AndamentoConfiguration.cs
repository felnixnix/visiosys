using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visiosys.Domain.Andamentos;

namespace Visiosys.Infrastructure.Persistence.Configurations;

public class AndamentoConfiguration : IEntityTypeConfiguration<Andamento>
{
    public void Configure(EntityTypeBuilder<Andamento> builder)
    {
        builder.ToTable("andamentos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.PrecatorioId).HasColumnName("precatorio_id").IsRequired();
        builder.HasIndex(a => a.PrecatorioId);

        builder.Property(a => a.Descricao)
            .HasColumnName("descricao").HasMaxLength(2000).IsRequired();

        builder.Property(a => a.Tipo).HasColumnName("tipo").IsRequired();

        builder.Property(a => a.RegistradoPorLogin)
            .HasColumnName("registrado_por_login").HasMaxLength(200).IsRequired();

        builder.Property(a => a.OcorridoEm).HasColumnName("ocorrido_em").IsRequired();
    }
}
