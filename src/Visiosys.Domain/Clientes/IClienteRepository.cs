namespace Visiosys.Domain.Clientes;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(string documento, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
