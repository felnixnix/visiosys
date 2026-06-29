namespace Visiosys.Domain.Documentos;

public interface IDocumentoRepository
{
    Task<Documento?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Documento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default);
    Task AdicionarAsync(Documento documento, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
