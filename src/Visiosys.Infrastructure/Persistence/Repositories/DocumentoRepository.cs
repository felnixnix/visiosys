using Microsoft.EntityFrameworkCore;
using Visiosys.Application.Documentos;
using Visiosys.Domain.Documentos;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class DocumentoRepository(VisiosysDbContext context) : IDocumentoRepository
{
    public async Task<Documento?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Documentos.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Documento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default)
        => await context.Documentos
            .Where(d => d.PrecatorioId == precatorioId)
            .OrderByDescending(d => d.CriadoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Documento documento, CancellationToken ct = default)
        => await context.Documentos.AddAsync(documento, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
