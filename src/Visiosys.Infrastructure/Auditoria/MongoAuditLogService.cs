using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Visiosys.Application.Auditoria;

namespace Visiosys.Infrastructure.Auditoria;

public class MongoAuditLogService : IAuditLogService
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoAuditLogService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("ConnectionStrings:Mongo não configurada.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("visiosys_auditoria");
        _collection = db.GetCollection<BsonDocument>("audit_logs");
    }

    public async Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default)
    {
        var doc = new BsonDocument
        {
            ["acao"]          = registro.Acao,
            ["usuario_login"] = registro.UsuarioLogin,
            ["entidade_tipo"] = registro.EntidadeTipo,
            ["entidade_id"]   = registro.EntidadeId,
            ["ocorrido_em"]   = registro.OcorridoEm
        };

        if (registro.Metadados is { Count: > 0 })
            doc["metadados"] = new BsonDocument(registro.Metadados);

        await _collection.InsertOneAsync(doc, null, ct);
    }
}
