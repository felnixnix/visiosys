using System.Threading.Channels;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Visiosys.Infrastructure.Logs;

/// <summary>
/// Sink Serilog próprio que grava os logs no MongoDB usando o mesmo
/// MongoDB.Driver da camada Infrastructure. Substitui o pacote
/// Serilog.Sinks.MongoDB, que era compilado contra uma versão diferente do
/// driver e quebrava em publish single-file (ver ADR-025).
///
/// O documento gerado casa exatamente com o formato lido por
/// <see cref="LogRepository"/> (Timestamp, Level, RenderedMessage,
/// Properties.* e Exception).
///
/// A escrita é assíncrona e em lote: os eventos entram numa fila limitada e
/// um worker de fundo faz InsertMany periodicamente, sem bloquear a thread
/// que emite o log (ex.: o pipeline de request logging).
/// </summary>
public sealed class MongoLogSink : ILogEventSink, IDisposable
{
    private const int TamanhoLote = 200;
    private const int LimiteFila = 5000;

    private readonly IMongoCollection<BsonDocument> _collection;
    private readonly Channel<LogEvent> _fila;
    private readonly Task _worker;

    public MongoLogSink(string connectionString, string collectionName)
    {
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        _collection = db.GetCollection<BsonDocument>(collectionName);

        // DropWrite: sob pico, prefere perder logs a travar a aplicação.
        _fila = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(LimiteFila)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

        _worker = Task.Run(ProcessarAsync);
    }

    public void Emit(LogEvent logEvent) => _fila.Writer.TryWrite(logEvent);

    private async Task ProcessarAsync()
    {
        var lote = new List<BsonDocument>(TamanhoLote);
        try
        {
            while (await _fila.Reader.WaitToReadAsync())
            {
                lote.Clear();
                while (lote.Count < TamanhoLote && _fila.Reader.TryRead(out var evento))
                    lote.Add(ParaDocumento(evento));

                if (lote.Count == 0)
                    continue;

                try
                {
                    await _collection.InsertManyAsync(lote);
                }
                catch (Exception ex)
                {
                    // Logging é não-crítico: falha ao gravar no Mongo não pode
                    // derrubar a aplicação. Reporta via SelfLog do Serilog.
                    SelfLog.WriteLine("MongoLogSink: falha ao gravar lote de logs: {0}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("MongoLogSink: worker encerrado com erro: {0}", ex);
        }
    }

    private static BsonDocument ParaDocumento(LogEvent logEvent)
    {
        var doc = new BsonDocument
        {
            ["Timestamp"] = logEvent.Timestamp.UtcDateTime,
            ["Level"] = logEvent.Level.ToString(),
            ["RenderedMessage"] = logEvent.RenderMessage()
        };

        if (logEvent.Exception is not null)
            doc["Exception"] = logEvent.Exception.ToString();

        if (logEvent.Properties.Count > 0)
        {
            var props = new BsonDocument();
            foreach (var (chave, valor) in logEvent.Properties)
                props[chave] = ParaBson(valor);
            doc["Properties"] = props;
        }

        return doc;
    }

    private static BsonValue ParaBson(LogEventPropertyValue valor)
    {
        if (valor is ScalarValue scalar)
        {
            return scalar.Value switch
            {
                null => BsonNull.Value,
                string s => new BsonString(s),
                bool b => new BsonBoolean(b),
                int i => new BsonInt32(i),
                long l => new BsonInt64(l),
                double d => new BsonDouble(d),
                decimal m => new BsonDecimal128(m),
                DateTime dt => new BsonDateTime(dt),
                DateTimeOffset dto => new BsonDateTime(dto.UtcDateTime),
                _ => new BsonString(scalar.Value.ToString())
            };
        }

        // Sequências/estruturas: guarda a representação textual do Serilog.
        return new BsonString(valor.ToString());
    }

    public void Dispose()
    {
        // Sinaliza o fim da fila e aguarda o worker drenar o que restou,
        // com timeout para não travar o shutdown da aplicação.
        _fila.Writer.TryComplete();
        try
        {
            _worker.Wait(TimeSpan.FromSeconds(5));
        }
        catch
        {
            // shutdown best-effort
        }
    }
}
