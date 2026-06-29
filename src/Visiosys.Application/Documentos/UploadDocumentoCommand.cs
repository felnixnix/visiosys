using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Application.Documentos;

public record UploadDocumentoCommand(
    string NomeOriginal,
    TipoDocumento Tipo,
    Stream Conteudo,
    long TamanhoBytes,
    string ContentType,
    string EnviadoPorLogin,
    Guid? PrecatorioId,
    Guid? ClienteId
);
