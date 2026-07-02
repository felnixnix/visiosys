using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;
using Visiosys.Domain.Precatorios.Queries;
using Visiosys.Infrastructure.Persistence;
using Visiosys.Infrastructure.Persistence.Repositories;

namespace Visiosys.Integration.Tests.Precatorios;

public class PrecatorioConsultaFiltroTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private VisiosysDbContext _db = null!;
    private PrecatorioRepository _repo = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<VisiosysDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _db = new VisiosysDbContext(options);
        await _db.Database.MigrateAsync();

        _db.Precatorios.AddRange(
            Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar),
            Precatorio.Criar("0009999-00.2021.4.03.0000", "TRF-3", 200_000m, EsferaPrecatorio.Federal, NaturezaPrecatorio.Comum),
            Precatorio.Criar("0005555-00.2019.8.13.0001", "TJMG", 50_000m, EsferaPrecatorio.Municipal, NaturezaPrecatorio.Comum));
        await _db.SaveChangesAsync();

        _repo = new PrecatorioRepository(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Filtro_PorNumeroParcial_DeveEncontrar()
    {
        var itens = await _repo.ListarAsync(new FiltroPrecatorios(Numero: "1234"));

        Assert.Single(itens);
        Assert.StartsWith("0001234", itens[0].Numero);
    }

    [Fact]
    public async Task Filtro_PorTribunal_DeveSerCaseInsensitive()
    {
        var itens = await _repo.ListarAsync(new FiltroPrecatorios(Tribunal: "trf"));

        Assert.Single(itens);
        Assert.Equal("TRF-3", itens[0].TribunalOrigem);
    }

    [Fact]
    public async Task Filtro_PorEsfera_DeveRetornarSomenteDaEsfera()
    {
        var itens = await _repo.ListarAsync(new FiltroPrecatorios(Esfera: EsferaPrecatorio.Federal));

        Assert.Single(itens);
        Assert.Equal(EsferaPrecatorio.Federal, itens[0].Esfera);
    }

    [Fact]
    public async Task Filtro_PorNatureza_DeveContarSomenteDaNatureza()
    {
        var total = await _repo.ContarAsync(new FiltroPrecatorios(Natureza: NaturezaPrecatorio.Comum));

        Assert.Equal(2, total);
    }

    [Fact]
    public async Task Filtro_Combinado_DeveAplicarTodosOsCriterios()
    {
        var filtro = new FiltroPrecatorios(Natureza: NaturezaPrecatorio.Comum, Esfera: EsferaPrecatorio.Municipal);
        var itens = await _repo.ListarAsync(filtro);

        Assert.Single(itens);
        Assert.Equal("TJMG", itens[0].TribunalOrigem);
    }

    [Fact]
    public async Task Filtro_Vazio_DeveContarTodos()
    {
        var total = await _repo.ContarAsync(new FiltroPrecatorios());

        Assert.Equal(3, total);
    }
}
