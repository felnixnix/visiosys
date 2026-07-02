using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;
using Visiosys.Domain.Precatorios.Queries;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class PrecatorioRepository(VisiosysDbContext context)
    : IPrecatorioRepository, IPrecatorioConsultaRepository
{
    public async Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Precatorios.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default)
        => await context.Precatorios.AnyAsync(p => p.Numero == numero, ct);

    public async Task AdicionarAsync(Precatorio precatorio, CancellationToken ct = default)
        => await context.Precatorios.AddAsync(precatorio, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Precatorio>> ListarAtivosAsync(CancellationToken ct = default)
        => await context.Precatorios
            .Where(p => p.Status != StatusPrecatorio.Liquidado && p.Status != StatusPrecatorio.Cancelado)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Precatorio>> ListarAsync(FiltroPrecatorios filtro, int skip = 0, int take = 50, CancellationToken ct = default)
        => await Filtrar(filtro)
            .OrderByDescending(p => p.CriadoEm)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public Task<int> ContarAsync(FiltroPrecatorios filtro, CancellationToken ct = default)
        => Filtrar(filtro).CountAsync(ct);

    // Monta a query com os filtros aplicados. Reutilizada por ListarAsync e
    // ContarAsync para que a contagem (paginação) reflita o mesmo recorte.
    private IQueryable<Precatorio> Filtrar(FiltroPrecatorios f)
    {
        var query = context.Precatorios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Numero))
            query = query.Where(p => EF.Functions.ILike(p.Numero, $"%{f.Numero}%"));

        if (!string.IsNullOrWhiteSpace(f.Tribunal))
            query = query.Where(p => EF.Functions.ILike(p.TribunalOrigem, $"%{f.Tribunal}%"));

        if (f.Esfera is not null)
            query = query.Where(p => p.Esfera == f.Esfera);

        if (f.Status is not null)
            query = query.Where(p => p.Status == f.Status);

        if (f.Natureza is not null)
            query = query.Where(p => p.Natureza == f.Natureza);

        return query;
    }
}
