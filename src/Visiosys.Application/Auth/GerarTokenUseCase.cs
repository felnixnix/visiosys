using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Visiosys.Application.Auth;

public class GerarTokenUseCase(IConfiguration config)
{
    public TokenDto Executar(LoginCommand command)
    {
        var adminValido = command.Login == config["Auth:Login"] && command.Senha == config["Auth:Senha"];
        var demoValido  = command.Login == config["Auth:DemoLogin"] && command.Senha == config["Auth:DemoSenha"];

        if (!adminValido && !demoValido)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Chave"]!));
        var expiracao = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiraEmMinutos"]!));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Emissor"],
            audience: config["Jwt:Emissor"],
            claims: [new Claim(ClaimTypes.Name, command.Login), new Claim(ClaimTypes.Role, "Operador")],
            expires: expiracao,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        );

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracao);
    }
}
