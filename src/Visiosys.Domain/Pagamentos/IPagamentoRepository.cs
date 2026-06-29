namespace Visiosys.Domain.Pagamentos;

public interface IPagamentoRepository
{
    Task<IReadOnlyList<Pagamento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default);
    Task AdicionarAsync(Pagamento pagamento, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
