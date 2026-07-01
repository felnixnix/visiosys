using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Visiosys.Application.Logs;
using Visiosys.Application.Precatorios;

namespace Visiosys.Infrastructure.Logs;

public class LogRepository
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public LogRepository(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("ConnectionStrings:Mongo não configurada.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        _collection = db.GetCollection<BsonDocument>("logs");
    }

    public async Task<PaginaDto<LogEntryDto>> ListarAsync(
        string? nivel, int pagina, int tamanho, CancellationToken ct)
    {
        tamanho = Math.Clamp(tamanho, 1, 100);
        pagina = Math.Max(1, pagina);

        var filtro = nivel is null
            ? Builders<BsonDocument>.Filter.Empty
            : Builders<BsonDocument>.Filter.Eq("Level", nivel);

        var total = (int)await _collection.CountDocumentsAsync(filtro, null, ct);

        var docs = await _collection
            .Find(filtro)
            .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
            .Skip((pagina - 1) * tamanho)
            .Limit(tamanho)
            .ToListAsync(ct);

        var items = docs.Select(Mapear).ToList();
        return new PaginaDto<LogEntryDto>(items, total, pagina, tamanho);
    }

    public async Task<LogStatsDto> ObterStatsAsync(CancellationToken ct)
    {
        var desde = DateTime.UtcNow.AddHours(-24);
        var filtroMatch = new BsonDocument("Timestamp", new BsonDocument("$gte", desde));

        // Contagem por nível (últimas 24h)
        var pipelineNivel = new[]
        {
            new BsonDocument("$match", filtroMatch),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = "$Level",
                ["total"] = new BsonDocument("$sum", 1)
            })
        };

        var resultadoNivel = await _collection
            .Aggregate<BsonDocument>(pipelineNivel, null, ct)
            .ToListAsync(ct);

        var porNivel = resultadoNivel
            .Select(d => new ContaPorNivelDto(
                d["_id"].AsString,
                d["total"].AsInt32))
            .ToList();

        int total24h = porNivel.Sum(x => x.Total);
        int erros24h = porNivel.Where(x => x.Nivel is "Error" or "Fatal").Sum(x => x.Total);
        int avisos24h = porNivel.Where(x => x.Nivel == "Warning").Sum(x => x.Total);

        // Requisições por hora (últimas 24h)
        var pipelineHora = new[]
        {
            new BsonDocument("$match", filtroMatch),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = new BsonDocument("$hour", "$Timestamp"),
                ["total"] = new BsonDocument("$sum", 1)
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var resultadoHora = await _collection
            .Aggregate<BsonDocument>(pipelineHora, null, ct)
            .ToListAsync(ct);

        var porHora = resultadoHora
            .Select(d => new ContaPorHoraDto(d["_id"].AsInt32, d["total"].AsInt32))
            .ToList();

        // Média de tempo de resposta (apenas logs de request com Elapsed)
        var pipelineElapsed = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                ["Timestamp"] = new BsonDocument("$gte", desde),
                ["Properties.Elapsed"] = new BsonDocument("$exists", true)
            }),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["media"] = new BsonDocument("$avg", "$Properties.Elapsed")
            })
        };

        var resultadoElapsed = await _collection
            .Aggregate<BsonDocument>(pipelineElapsed, null, ct)
            .ToListAsync(ct);

        var mediaElapsed = resultadoElapsed.Count > 0
            ? Math.Round(resultadoElapsed[0]["media"].ToDouble(), 1)
            : 0;

        return new LogStatsDto(total24h, erros24h, avisos24h, mediaElapsed, porHora, porNivel);
    }

    private static LogEntryDto Mapear(BsonDocument doc)
    {
        var props = doc.Contains("Properties") && doc["Properties"].IsBsonDocument
            ? doc["Properties"].AsBsonDocument
            : null;

        return new LogEntryDto(
            Timestamp: doc["Timestamp"].ToUniversalTime(),
            Level: doc.GetValue("Level", "Information").AsString,
            Mensagem: doc.GetValue("RenderedMessage", "").AsString,
            Metodo: props?.GetValue("RequestMethod", BsonNull.Value).IsBsonNull == false
                ? props["RequestMethod"].AsString : null,
            Caminho: props?.GetValue("RequestPath", BsonNull.Value).IsBsonNull == false
                ? props["RequestPath"].AsString : null,
            StatusCode: props?.Contains("StatusCode") == true && !props["StatusCode"].IsBsonNull
                ? props["StatusCode"].ToInt32() : null,
            ElapsedMs: props?.Contains("Elapsed") == true && !props["Elapsed"].IsBsonNull
                ? Math.Round(props["Elapsed"].ToDouble(), 1) : null,
            Excecao: doc.GetValue("Exception", BsonNull.Value).IsBsonNull
                ? null : doc["Exception"].AsString
        );
    }
}
