namespace Visiosys.Domain.Precatorios;

public interface IPrecatorioRepository
{
    Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default);
    Task AdicionarAsync(Precatorio precatorio, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
