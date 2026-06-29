using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visiosys.Domain.Precatorios;

namespace Visiosys.Infrastructure.Persistence.Configurations;

public class PrecatorioConfiguration : IEntityTypeConfiguration<Precatorio>
{
    public void Configure(EntityTypeBuilder<Precatorio> builder)
    {
        builder.ToTable("precatorios");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Numero)
            .HasColumnName("numero")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Numero)
            .IsUnique();

        builder.Property(p => p.TribunalOrigem)
            .HasColumnName("tribunal_origem")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ValorFace)
            .HasColumnName("valor_face")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.ValorAtualizado)
            .HasColumnName("valor_atualizado")
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Esfera)
            .HasColumnName("esfera")
            .IsRequired();

        builder.Property(p => p.Natureza)
            .HasColumnName("natureza")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(p => p.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        // Concorrência Otimista — impede Lost Updates (RNF15)
        builder.Property(p => p.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();
    }
}
