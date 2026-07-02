namespace Visiosys.Domain.Clientes;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(FiltroClientes filtro, int skip = 0, int take = 20, CancellationToken ct = default);
    Task<int> ContarAsync(FiltroClientes filtro, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(string documento, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
