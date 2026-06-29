using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Precatorios;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/precatorios")]
public class PrecatoriosController(
    CriarPrecatorioUseCase criarUseCase,
    ObterPrecatorioPorIdUseCase obterUseCase,
    ListarPrecatoriosUseCase listarUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<PrecatorioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var resultado = await listarUseCase.ExecutarAsync(pagina, tamanho, ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PrecatorioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPrecatorioCommand command, CancellationToken ct)
    {
        try
        {
            var dto = await criarUseCase.ExecutarAsync(command, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrecatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var dto = await obterUseCase.ExecutarAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
