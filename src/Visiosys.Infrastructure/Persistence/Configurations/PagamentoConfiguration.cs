using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visiosys.Domain.Pagamentos;

namespace Visiosys.Infrastructure.Persistence.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("pagamentos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.PrecatorioId).HasColumnName("precatorio_id").IsRequired();
        builder.HasIndex(p => p.PrecatorioId);

        builder.Property(p => p.ValorPago)
            .HasColumnName("valor_pago").HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(p => p.ValorBase)
            .HasColumnName("valor_base").HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(p => p.PercDesagio)
            .HasColumnName("perc_desagio").HasColumnType("numeric(7,4)").IsRequired();

        builder.Property(p => p.RegistradoPorLogin)
            .HasColumnName("registrado_por_login").HasMaxLength(200).IsRequired();

        builder.Property(p => p.PagoEm).HasColumnName("pago_em").IsRequired();
        builder.Property(p => p.CriadoEm).HasColumnName("criado_em").IsRequired();
    }
}
