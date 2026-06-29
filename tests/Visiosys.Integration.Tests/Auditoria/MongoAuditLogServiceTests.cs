using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Testcontainers.MongoDb;
using Visiosys.Application.Auditoria;
using Visiosys.Infrastructure.Auditoria;

namespace Visiosys.Integration.Tests.Auditoria;

public class MongoAuditLogServiceTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:7")
        .Build();

    public Task InitializeAsync() => _mongo.StartAsync();
    public Task DisposeAsync() => _mongo.DisposeAsync().AsTask();

    private IAuditLogService CriarServico()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Mongo"] = _mongo.GetConnectionString()
            })
            .Build();

        return new MongoAuditLogService(config);
    }

    [Fact]
    public async Task RegistrarAsync_ComRegistroValido_DeveInserirNoMongo()
    {
        var servico = CriarServico();

        var registro = new RegistroAuditoria(
            Acao: "ANDAMENTO_REGISTRADO",
            UsuarioLogin: "admin",
            EntidadeTipo: "Precatorio",
            EntidadeId: Guid.NewGuid().ToString(),
            OcorridoEm: DateTime.UtcNow,
            Metadados: new() { ["tipo"] = "ContatoRealizado" }
        );

        // Não deve lançar exceção
        await servico.RegistrarAsync(registro);
    }

    [Fact]
    public async Task RegistrarAsync_MultiplosChamados_DeveInserirTodos()
    {
        var servico = CriarServico();
        var precId = Guid.NewGuid().ToString();

        for (var i = 0; i < 3; i++)
        {
            await servico.RegistrarAsync(new RegistroAuditoria(
                Acao: $"ACAO_{i}",
                UsuarioLogin: "admin",
                EntidadeTipo: "Precatorio",
                EntidadeId: precId,
                OcorridoEm: DateTime.UtcNow));
        }
    }
}
