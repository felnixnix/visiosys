using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Visiosys.Infrastructure.Logs;

namespace Visiosys.Integration.Tests.Logs;

public class LogRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:7")
        .Build();

    public Task InitializeAsync() => _mongo.StartAsync();
    public Task DisposeAsync() => _mongo.DisposeAsync().AsTask();

    // O LogRepository deriva o banco do próprio connection string (ao contrário
    // do MongoAuditLogService, que fixa o nome). O connection string do
    // Testcontainers não traz banco, então acrescentamos um — mantendo
    // authSource=admin, onde o usuário root do container é criado.
    private string ConnString()
    {
        return new MongoUrlBuilder(_mongo.GetConnectionString())
        {
            DatabaseName = "visiosys_auditoria",
            AuthenticationSource = "admin"
        }.ToString();
    }

    private static LogRepository CriarRepositorio(string conn)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Mongo"] = conn
            })
            .Build();

        return new LogRepository(config);
    }

    private static IMongoCollection<BsonDocument> Colecao(string conn)
    {
        var client = new MongoClient(conn);
        var db = client.GetDatabase(MongoUrl.Create(conn).DatabaseName);
        return db.GetCollection<BsonDocument>("logs");
    }

    // Reproduz o formato gravado pelo MongoLogSink (ver ADR-025).
    private static BsonDocument LogDoc(
        string level, DateTime ts,
        string? metodo = null, string? caminho = null,
        int? status = null, double? elapsed = null)
    {
        var doc = new BsonDocument
        {
            ["Timestamp"] = ts,
            ["Level"] = level,
            ["RenderedMessage"] = $"{level} {caminho ?? "-"}"
        };

        var props = new BsonDocument();
        if (metodo is not null) props["RequestMethod"] = metodo;
        if (caminho is not null) props["RequestPath"] = caminho;
        if (status is not null) props["StatusCode"] = status.Value;
        if (elapsed is not null) props["Elapsed"] = elapsed.Value;
        if (props.ElementCount > 0) doc["Properties"] = props;

        return doc;
    }

    [Fact]
    public async Task ListarAsync_DeveMapearCamposEOrdenarDoMaisRecente()
    {
        var conn = ConnString();
        var agora = DateTime.UtcNow;
        await Colecao(conn).InsertManyAsync(new[]
        {
            LogDoc("Information", agora, "GET", "/api/precatorios", 200, 12.5),
            LogDoc("Warning", agora.AddMinutes(-1), "POST", "/api/auth/login", 401, 3.2),
            LogDoc("Error", agora.AddMinutes(-2)),
        });

        var pagina = await CriarRepositorio(conn).ListarAsync(null, 1, 10, default);

        Assert.Equal(3, pagina.Total);
        var maisRecente = pagina.Items[0];
        Assert.Equal("Information", maisRecente.Level);
        Assert.Equal("GET", maisRecente.Metodo);
        Assert.Equal("/api/precatorios", maisRecente.Caminho);
        Assert.Equal(200, maisRecente.StatusCode);
        Assert.Equal(12.5, maisRecente.ElapsedMs!.Value);
    }

    [Fact]
    public async Task ListarAsync_ComFiltroDeNivel_DeveRetornarSomenteDoNivel()
    {
        var conn = ConnString();
        var agora = DateTime.UtcNow;
        await Colecao(conn).InsertManyAsync(new[]
        {
            LogDoc("Information", agora),
            LogDoc("Error", agora.AddSeconds(-1)),
            LogDoc("Error", agora.AddSeconds(-2)),
        });

        var pagina = await CriarRepositorio(conn).ListarAsync("Error", 1, 10, default);

        Assert.Equal(2, pagina.Total);
        Assert.All(pagina.Items, i => Assert.Equal("Error", i.Level));
    }

    [Fact]
    public async Task ObterStatsAsync_DeveAgregarContagensEMediaDeTempo()
    {
        var conn = ConnString();
        var agora = DateTime.UtcNow;
        await Colecao(conn).InsertManyAsync(new[]
        {
            LogDoc("Information", agora, "GET", "/a", 200, 10.0),
            LogDoc("Information", agora.AddMinutes(-5), "GET", "/b", 200, 20.0),
            LogDoc("Warning", agora.AddMinutes(-10)),
            LogDoc("Error", agora.AddMinutes(-15)),
            LogDoc("Fatal", agora.AddMinutes(-20)),
        });

        var stats = await CriarRepositorio(conn).ObterStatsAsync(default);

        Assert.Equal(5, stats.TotalRequests24h);
        Assert.Equal(2, stats.Erros24h);  // Error + Fatal
        Assert.Equal(1, stats.Avisos24h); // Warning
        Assert.Equal(15.0, stats.MediaElapsedMs); // média de 10 e 20
    }

    [Fact]
    public async Task ObterStatsAsync_DeveIgnorarEventosForaDaJanelaDe24h()
    {
        var conn = ConnString();
        var agora = DateTime.UtcNow;
        await Colecao(conn).InsertManyAsync(new[]
        {
            LogDoc("Information", agora),
            LogDoc("Information", agora.AddHours(-30)),
        });

        var stats = await CriarRepositorio(conn).ObterStatsAsync(default);

        Assert.Equal(1, stats.TotalRequests24h);
    }
}
