using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Precatorios;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class PrecatorioRepository(VisiosysDbContext context) : IPrecatorioRepository
{
    public async Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Precatorios.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default)
        => await context.Precatorios.AnyAsync(p => p.Numero == numero, ct);

    public async Task AdicionarAsync(Precatorio precatorio, CancellationToken ct = default)
        => await context.Precatorios.AddAsync(precatorio, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
