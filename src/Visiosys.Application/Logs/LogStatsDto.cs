namespace Visiosys.Application.Logs;

public record LogStatsDto(
    int TotalRequests24h,
    int Erros24h,
    int Avisos24h,
    double MediaElapsedMs,
    IReadOnlyList<ContaPorHoraDto> PorHora,
    IReadOnlyList<ContaPorNivelDto> PorNivel
);

public record ContaPorHoraDto(int Hora, int Total);
public record ContaPorNivelDto(string Nivel, int Total);
