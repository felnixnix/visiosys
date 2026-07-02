using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Clientes;
using Visiosys.Application.Precatorios;
using Visiosys.Domain.Clientes;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClientesController(
    CriarClienteUseCase criarUseCase,
    ObterClientePorIdUseCase obterUseCase,
    ListarClientesUseCase listarUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nome = null,
        [FromQuery] string? documento = null,
        [FromQuery] string? tipo = null,
        [FromQuery] string? letra = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var filtro = new FiltroClientes(nome, documento, tipo, letra);
        var resultado = await listarUseCase.ExecutarAsync(filtro, pagina, tamanho, ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteCommand command, CancellationToken ct)
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
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var dto = await obterUseCase.ExecutarAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
