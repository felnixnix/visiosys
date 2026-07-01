using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Visiosys.Infrastructure.Logs;

/// <summary>
/// Extensão de configuração para o <see cref="MongoLogSink"/>.
///
/// Assinatura pública expõe apenas tipos do Serilog e strings — não expõe
/// tipos do MongoDB.Driver. Isso evita o erro de compilação CS0012 no projeto
/// Api (que não referencia o driver) e permite configurar o sink em código,
/// sem descoberta de assembly por reflection (que falha em single-file).
/// </summary>
public static class MongoDbLogsSinkExtensions
{
    public static LoggerConfiguration MongoDbLogs(
        this LoggerSinkConfiguration sinkConfiguration,
        string connectionString,
        string collectionName = "logs",
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
    {
        var sink = new MongoLogSink(connectionString, collectionName);
        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }
}
