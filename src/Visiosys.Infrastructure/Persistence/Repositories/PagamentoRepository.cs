using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Pagamentos;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class PagamentoRepository(VisiosysDbContext context) : IPagamentoRepository
{
    public async Task<IReadOnlyList<Pagamento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default)
        => await context.Pagamentos
            .Where(p => p.PrecatorioId == precatorioId)
            .OrderByDescending(p => p.PagoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Pagamento pagamento, CancellationToken ct = default)
        => await context.Pagamentos.AddAsync(pagamento, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
