using Microsoft.EntityFrameworkCore;
using Visiosys.Domain.Precatorios;

namespace Visiosys.Infrastructure.Persistence;

public class VisiosysDbContext : DbContext
{
    public VisiosysDbContext(DbContextOptions<VisiosysDbContext> options) : base(options) { }

    public DbSet<Precatorio> Precatorios => Set<Precatorio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VisiosysDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
