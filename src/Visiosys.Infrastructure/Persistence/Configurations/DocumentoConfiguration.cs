using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visiosys.Domain.Documentos;

namespace Visiosys.Infrastructure.Persistence.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("documentos");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.NomeOriginal)
            .HasColumnName("nome_original").HasMaxLength(500).IsRequired();

        builder.Property(d => d.Tipo).HasColumnName("tipo").IsRequired();

        builder.Property(d => d.ChaveArmazenamento)
            .HasColumnName("chave_armazenamento").HasMaxLength(1000).IsRequired();

        builder.Property(d => d.UrlDownload)
            .HasColumnName("url_download").HasMaxLength(2048).IsRequired();

        builder.Property(d => d.TamanhoBytes).HasColumnName("tamanho_bytes").IsRequired();

        builder.Property(d => d.ContentType)
            .HasColumnName("content_type").HasMaxLength(100).IsRequired();

        builder.Property(d => d.EnviadoPorLogin)
            .HasColumnName("enviado_por_login").HasMaxLength(200).IsRequired();

        builder.Property(d => d.PrecatorioId).HasColumnName("precatorio_id");
        builder.Property(d => d.ClienteId).HasColumnName("cliente_id");

        builder.Property(d => d.CriadoEm).HasColumnName("criado_em").IsRequired();
    }
}
