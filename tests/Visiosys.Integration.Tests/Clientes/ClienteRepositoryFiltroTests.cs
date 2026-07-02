using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Visiosys.Domain.Clientes;
using Visiosys.Infrastructure.Persistence;
using Visiosys.Infrastructure.Persistence.Repositories;

namespace Visiosys.Integration.Tests.Clientes;

public class ClienteRepositoryFiltroTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private VisiosysDbContext _db = null!;
    private ClienteRepository _repo = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<VisiosysDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _db = new VisiosysDbContext(options);
        await _db.Database.MigrateAsync();

        _db.Clientes.AddRange(
            Cliente.Criar("Ana Souza", "52998224725", "ana@x.com"),
            Cliente.Criar("Bruno Lima", "11144477735", "bruno@x.com"),
            Cliente.Criar("Carlos Alves", "12345678909", "carlos@x.com"),
            Cliente.Criar("Empresa Delta Ltda", "11222333000181", "delta@x.com"));
        await _db.SaveChangesAsync();

        _repo = new ClienteRepository(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Filtro_PorNome_DeveSerContainsCaseInsensitive()
    {
        var filtro = new FiltroClientes(Nome: "lima");
        var itens = await _repo.ListarAsync(filtro);
        var total = await _repo.ContarAsync(filtro);

        Assert.Equal(1, total);
        Assert.Single(itens);
        Assert.Equal("Bruno Lima", itens[0].Nome);
    }

    [Fact]
    public async Task Filtro_PorLetraInicial_DeveRetornarSomenteComAInicial()
    {
        var itens = await _repo.ListarAsync(new FiltroClientes(Letra: "A"));

        Assert.Single(itens);
        Assert.Equal("Ana Souza", itens[0].Nome);
    }

    [Fact]
    public async Task Filtro_PorTipoPJ_DeveRetornarSomenteCnpj()
    {
        var itens = await _repo.ListarAsync(new FiltroClientes(Tipo: "PJ"));

        Assert.Single(itens);
        Assert.Equal(14, itens[0].Documento.Length);
    }

    [Fact]
    public async Task Filtro_PorTipoPF_DeveContarSomenteCpf()
    {
        var total = await _repo.ContarAsync(new FiltroClientes(Tipo: "PF"));

        Assert.Equal(3, total);
    }

    [Fact]
    public async Task Filtro_PorDocumentoComMascara_DeveIgnorarPontuacao()
    {
        var itens = await _repo.ListarAsync(new FiltroClientes(Documento: "529.982"));

        Assert.Single(itens);
        Assert.Equal("Ana Souza", itens[0].Nome);
    }

    [Fact]
    public async Task Filtro_Vazio_DeveContarTodos()
    {
        var total = await _repo.ContarAsync(new FiltroClientes());

        Assert.Equal(4, total);
    }
}
