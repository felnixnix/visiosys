using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Andamentos;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/precatorios/{precatorioId:guid}/andamentos")]
public class AndamentosController(
    RegistrarAndamentoUseCase registrarUseCase,
    ListarAndamentosUseCase listarUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AndamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar(
        Guid precatorioId,
        [FromBody] RegistrarAndamentoRequest request,
        CancellationToken ct)
    {
        var login = User.Identity?.Name ?? "desconhecido";
        var command = new RegistrarAndamentoCommand(precatorioId, request.Descricao, request.Tipo, login);

        try
        {
            var dto = await registrarUseCase.ExecutarAsync(command, ct);
            return CreatedAtAction(nameof(Listar), new { precatorioId }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AndamentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(Guid precatorioId, CancellationToken ct)
    {
        var dtos = await listarUseCase.ExecutarAsync(precatorioId, ct);
        return Ok(dtos);
    }
}
