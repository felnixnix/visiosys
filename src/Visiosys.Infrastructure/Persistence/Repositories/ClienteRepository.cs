using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Clientes;

namespace Visiosys.Infrastructure.Persistence.Repositories;

public class ClienteRepository(VisiosysDbContext context) : IClienteRepository
{
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(FiltroClientes filtro, int skip = 0, int take = 20, CancellationToken ct = default)
        => await Filtrar(filtro)
            .OrderBy(c => c.Nome)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> ContarAsync(FiltroClientes filtro, CancellationToken ct = default)
        => Filtrar(filtro).CountAsync(ct);

    // Monta a query com os filtros aplicados. Reutilizada por ListarAsync e
    // ContarAsync para que a contagem (paginação) reflita o mesmo recorte.
    private IQueryable<Cliente> Filtrar(FiltroClientes f)
    {
        var query = context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Nome))
            query = query.Where(c => EF.Functions.ILike(c.Nome, $"%{f.Nome}%"));

        if (!string.IsNullOrWhiteSpace(f.Documento))
        {
            var digitos = new string(f.Documento.Where(char.IsDigit).ToArray());
            if (digitos.Length > 0)
                query = query.Where(c => c.Documento.Contains(digitos));
        }

        if (f.Tipo == "PF")
            query = query.Where(c => c.Documento.Length == 11);
        else if (f.Tipo == "PJ")
            query = query.Where(c => c.Documento.Length == 14);

        if (!string.IsNullOrWhiteSpace(f.Letra))
            query = query.Where(c => EF.Functions.ILike(c.Nome, $"{f.Letra}%"));

        return query;
    }

    public async Task<bool> ExisteDocumentoAsync(string documento, CancellationToken ct = default)
        => await context.Clientes.AnyAsync(c => c.Documento == documento, ct);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default)
        => await context.Clientes.AddAsync(cliente, ct);

    public Task SalvarAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
