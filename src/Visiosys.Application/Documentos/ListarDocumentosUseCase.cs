using Visiosys.Domain.Documentos;

namespace Visiosys.Application.Documentos;

public class ListarDocumentosUseCase(IDocumentoRepository repository)
{
    public async Task<IReadOnlyList<DocumentoDto>> ExecutarAsync(Guid precatorioId, CancellationToken ct = default)
    {
        var docs = await repository.ListarPorPrecatorioAsync(precatorioId, ct);
        return docs.Select(DocumentoDto.DeEntidade).ToList();
    }
}
