namespace Visiosys.Domain.Andamentos;

public interface IAndamentoRepository
{
    Task<IReadOnlyList<Andamento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default);
    Task AdicionarAsync(Andamento andamento, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
