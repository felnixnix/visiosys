using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Andamentos;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class AndamentoRepository(VisiosysDbContext context) : IAndamentoRepository
{
    public async Task<IReadOnlyList<Andamento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default)
        => await context.Andamentos
            .Where(a => a.PrecatorioId == precatorioId)
            .OrderByDescending(a => a.OcorridoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Andamento andamento, CancellationToken ct = default)
        => await context.Andamentos.AddAsync(andamento, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
