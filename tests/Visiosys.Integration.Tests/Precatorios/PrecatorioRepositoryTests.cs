using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;
using Visiosys.Infrastructure.Persistence;

namespace Visiosys.Integration.Tests.Precatorios;

public class PrecatorioRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private VisiosysDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<VisiosysDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new VisiosysDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Persistir_PrecatorioValido_DeveRecuperarComMesmosValores()
    {
        var precatorio = Precatorio.Criar(
            "0001234-56.2020.8.26.0100",
            "TJSP",
            500_000m,
            EsferaPrecatorio.Estadual,
            NaturezaPrecatorio.Alimentar
        );

        _dbContext.Precatorios.Add(precatorio);
        await _dbContext.SaveChangesAsync();

        var recuperado = await _dbContext.Precatorios
            .FirstOrDefaultAsync(p => p.Numero == "0001234-56.2020.8.26.0100");

        Assert.NotNull(recuperado);
        Assert.Equal(precatorio.Id, recuperado.Id);
        Assert.Equal(500_000m, recuperado.ValorFace);
        Assert.Equal(EsferaPrecatorio.Estadual, recuperado.Esfera);
        Assert.Equal(StatusPrecatorio.EmAnalise, recuperado.Status);
    }

    [Fact]
    public async Task Persistir_DoisPrecatoriosComMesmoNumero_DeveLancarExcecao()
    {
        var p1 = Precatorio.Criar("DUPLICADO-001", "TJSP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Comum);
        var p2 = Precatorio.Criar("DUPLICADO-001", "TJMG", 200_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Comum);

        _dbContext.Precatorios.Add(p1);
        await _dbContext.SaveChangesAsync();

        _dbContext.Precatorios.Add(p2);
        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Atualizar_ComEdicaoSimultanea_DeveLancarConcorrenciaOtimista()
    {
        var precatorio = Precatorio.Criar(
            "CONCORRENCIA-001", "STF", 1_000_000m, EsferaPrecatorio.Federal, NaturezaPrecatorio.Alimentar
        );
        _dbContext.Precatorios.Add(precatorio);
        await _dbContext.SaveChangesAsync();

        // Simula dois contextos lendo o mesmo registro simultaneamente
        var options = new DbContextOptionsBuilder<VisiosysDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using var contextoA = new VisiosysDbContext(options);
        await using var contextoB = new VisiosysDbContext(options);

        var precatorioA = await contextoA.Precatorios.FirstAsync(p => p.Numero == "CONCORRENCIA-001");
        var precatorioB = await contextoB.Precatorios.FirstAsync(p => p.Numero == "CONCORRENCIA-001");

        // Contexto A salva primeiro
        precatorioA.AtualizarValorAtualizado(1_100_000m);
        await contextoA.SaveChangesAsync();

        // Contexto B tenta salvar com o RowVersion desatualizado — deve falhar
        precatorioB.AtualizarValorAtualizado(1_200_000m);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextoB.SaveChangesAsync()
        );
    }
}
