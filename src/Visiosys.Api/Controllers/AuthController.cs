using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Visiosys.Application.Auth;

namespace Visiosys.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(GerarTokenUseCase gerarTokenUseCase) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginCommand command)
    {
        try
        {
            var dto = gerarTokenUseCase.Executar(command);
            return Ok(dto);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { erro = "Credenciais inválidas." });
        }
    }
}
