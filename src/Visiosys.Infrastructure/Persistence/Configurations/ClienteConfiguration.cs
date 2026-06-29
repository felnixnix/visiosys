using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visiosys.Domain.Clientes;

namespace Visiosys.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Nome)
            .HasColumnName("nome").HasMaxLength(200).IsRequired();

        builder.Property(c => c.Documento)
            .HasColumnName("documento").HasMaxLength(14).IsRequired();

        builder.HasIndex(c => c.Documento).IsUnique();

        builder.Property(c => c.Email)
            .HasColumnName("email").HasMaxLength(200).IsRequired();

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone").HasMaxLength(20);

        builder.OwnsOne(c => c.DadosBancarios, db =>
        {
            db.Property(x => x.Banco).HasColumnName("banco_codigo").HasMaxLength(10);
            db.Property(x => x.Agencia).HasColumnName("banco_agencia").HasMaxLength(10);
            db.Property(x => x.NumeroConta).HasColumnName("banco_numero_conta").HasMaxLength(20);
            db.Property(x => x.TipoConta).HasColumnName("banco_tipo_conta");
        });

        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.Property(c => c.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();

#pragma warning disable CS0618
        builder.UseXminAsConcurrencyToken();
#pragma warning restore CS0618
    }
}
