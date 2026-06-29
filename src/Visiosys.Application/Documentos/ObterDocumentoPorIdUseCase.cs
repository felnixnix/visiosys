using Visiosys.Domain.Documentos;

namespace Visiosys.Application.Documentos;

public class ObterDocumentoPorIdUseCase(IDocumentoRepository repository)
{
    public async Task<DocumentoDto?> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await repository.ObterPorIdAsync(id, ct);
        return doc is null ? null : DocumentoDto.DeEntidade(doc);
    }
}
