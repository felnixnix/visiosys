using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Clientes;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class ClienteRepository(VisiosysDbContext context) : IClienteRepository
{
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExisteDocumentoAsync(string documento, CancellationToken ct = default)
        => await context.Clientes.AnyAsync(c => c.Documento == documento, ct);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default)
        => await context.Clientes.AddAsync(cliente, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
