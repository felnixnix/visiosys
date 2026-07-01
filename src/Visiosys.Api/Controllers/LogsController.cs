using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Logs;
using Visiosys.Application.Precatorios;
using Visiosys.Infrastructure.Logs;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/logs")]
public class LogsController(LogRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<LogEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nivel,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var resultado = await repository.ListarAsync(nivel, pagina, tamanho, ct);
        return Ok(resultado);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(LogStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        var resultado = await repository.ObterStatsAsync(ct);
        return Ok(resultado);
    }
}
