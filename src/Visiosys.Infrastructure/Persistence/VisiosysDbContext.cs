using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Clientes;
using Visiosys.Domain.Precatorios;

namespace Visiosys.Infrastructure.Persistence;

public class VisiosysDbContext : DbContext
{
    public VisiosysDbContext(DbContextOptions<VisiosysDbContext> options) : base(options) { }

    public DbSet<Precatorio> Precatorios => Set<Precatorio>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VisiosysDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
