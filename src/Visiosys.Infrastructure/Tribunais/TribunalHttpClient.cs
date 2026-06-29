using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Visiosys.Application.Tribunais;

namespace Visiosys.Infrastructure.Tribunais;

// Adaptador HTTP para os tribunais. Cada tribunal expõe uma URL diferente;
// esta implementação usa uma URL base configurável em Worker:TribunalBaseUrl.
// Em dev (sem URL configurada) retorna lista vazia e loga aviso.
public class TribunalHttpClient(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<TribunalHttpClient> logger) : IConsultaTribunalService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<AndamentoTribunalDto>> ConsultarAndamentosAsync(
        string numeroPrecatorio, CancellationToken ct = default)
    {
        var baseUrl = config["Worker:TribunalBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            logger.LogWarning("Worker:TribunalBaseUrl não configurada. Consulta omitida para {Numero}.", numeroPrecatorio);
            return [];
        }

        var url = $"{baseUrl.TrimEnd('/')}/andamentos/{Uri.EscapeDataString(numeroPrecatorio)}";
        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<List<AndamentoTribunalDto>>(stream, JsonOpts, ct)
               ?? [];
    }
}
