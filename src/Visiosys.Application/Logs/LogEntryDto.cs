namespace Visiosys.Application.Logs;

public record LogEntryDto(
    DateTime Timestamp,
    string Level,
    string Mensagem,
    string? Metodo,
    string? Caminho,
    int? StatusCode,
    double? ElapsedMs,
    string? Excecao
);
