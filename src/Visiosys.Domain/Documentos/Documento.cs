using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Domain.Documentos;

public class Documento
{
    public Guid Id { get; private set; }
    public string NomeOriginal { get; private set; } = null!;
    public TipoDocumento Tipo { get; private set; }
    public string ChaveArmazenamento { get; private set; } = null!;
    public string UrlDownload { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string ContentType { get; private set; } = null!;
    public string EnviadoPorLogin { get; private set; } = null!;
    public Guid? PrecatorioId { get; private set; }
    public Guid? ClienteId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Documento() { }

    public static Documento Criar(
        string nomeOriginal,
        TipoDocumento tipo,
        string chaveArmazenamento,
        string urlDownload,
        long tamanhoBytes,
        string contentType,
        string enviadoPorLogin,
        Guid? precatorioId = null,
        Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeOriginal))
            throw new ArgumentException("O nome do arquivo é obrigatório.", nameof(nomeOriginal));
        if (string.IsNullOrWhiteSpace(chaveArmazenamento))
            throw new ArgumentException("A chave de armazenamento é obrigatória.", nameof(chaveArmazenamento));
        if (tamanhoBytes <= 0)
            throw new ArgumentException("O tamanho do arquivo deve ser maior que zero.", nameof(tamanhoBytes));
        if (precatorioId is null && clienteId is null)
            throw new ArgumentException("O documento deve estar associado a um precatório ou a um cliente.");

        return new Documento
        {
            Id = Guid.NewGuid(),
            NomeOriginal = nomeOriginal.Trim(),
            Tipo = tipo,
            ChaveArmazenamento = chaveArmazenamento,
            UrlDownload = urlDownload,
            TamanhoBytes = tamanhoBytes,
            ContentType = contentType,
            EnviadoPorLogin = enviadoPorLogin,
            PrecatorioId = precatorioId,
            ClienteId = clienteId,
            CriadoEm = DateTime.UtcNow
        };
    }
}
