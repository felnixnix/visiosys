using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visiosys.Application.Documentos;
using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/documentos")]
public class DocumentosController(
    UploadDocumentoUseCase uploadUseCase,
    ObterDocumentoPorIdUseCase obterUseCase,
    ListarDocumentosUseCase listarUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DocumentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] TipoDocumento tipo,
        [FromForm] Guid? precatorioId,
        [FromForm] Guid? clienteId,
        IFormFile arquivo,
        CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { erro = "Nenhum arquivo enviado." });

        var login = User.Identity?.Name ?? "desconhecido";

        var command = new UploadDocumentoCommand(
            NomeOriginal: arquivo.FileName,
            Tipo: tipo,
            Conteudo: arquivo.OpenReadStream(),
            TamanhoBytes: arquivo.Length,
            ContentType: arquivo.ContentType,
            EnviadoPorLogin: login,
            PrecatorioId: precatorioId,
            ClienteId: clienteId
        );

        try
        {
            var dto = await uploadUseCase.ExecutarAsync(command, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = dto.Id }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] Guid precatorioId, CancellationToken ct)
    {
        var dtos = await listarUseCase.ExecutarAsync(precatorioId, ct);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var dto = await obterUseCase.ExecutarAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
