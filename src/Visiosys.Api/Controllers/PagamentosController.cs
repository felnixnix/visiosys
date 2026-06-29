using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Pagamentos;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/precatorios/{precatorioId:guid}/pagamentos")]
public class PagamentosController(
    RegistrarPagamentoUseCase registrarUseCase,
    ListarPagamentosUseCase listarUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PagamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        Guid precatorioId,
        [FromBody] RegistrarPagamentoRequest request,
        CancellationToken ct)
    {
        var login = User.Identity?.Name ?? "desconhecido";
        var command = new RegistrarPagamentoCommand(precatorioId, request.ValorPago, login, request.PagoEm);

        try
        {
            var dto = await registrarUseCase.ExecutarAsync(command, ct);
            return CreatedAtAction(nameof(Listar), new { precatorioId }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PagamentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(Guid precatorioId, CancellationToken ct)
    {
        var dtos = await listarUseCase.ExecutarAsync(precatorioId, ct);
        return Ok(dtos);
    }
}
