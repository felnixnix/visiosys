using Visiosys.Domain.Documentos;
using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Application.Documentos;

public record DocumentoDto(
    Guid Id,
    string NomeOriginal,
    TipoDocumento Tipo,
    string UrlDownload,
    long TamanhoBytes,
    string ContentType,
    string EnviadoPorLogin,
    Guid? PrecatorioId,
    Guid? ClienteId,
    DateTime CriadoEm
)
{
    public static DocumentoDto DeEntidade(Documento d) => new(
        d.Id, d.NomeOriginal, d.Tipo, d.UrlDownload,
        d.TamanhoBytes, d.ContentType, d.EnviadoPorLogin,
        d.PrecatorioId, d.ClienteId, d.CriadoEm);
}
