using Microsoft.Extensions.Configuration;
using Visiosys.Application.Auth;

namespace Visiosys.Domain.Tests.Application;

public class GerarTokenUseCaseTests
{
    private static GerarTokenUseCase CriarUseCase(string login = "admin", string senha = "Visiosys@Dev1") =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Login"]           = login,
                ["Auth:Senha"]           = senha,
                ["Jwt:Chave"]            = "chave-secreta-minima-32-chars-visiosys!!",
                ["Jwt:Emissor"]          = "visiosys-test",
                ["Jwt:ExpiraEmMinutos"]  = "60"
            })
            .Build());

    [Fact]
    public void Executar_ComCredenciaisValidas_DeveRetornarTokenNaoVazio()
    {
        var dto = CriarUseCase().Executar(new LoginCommand("admin", "Visiosys@Dev1"));

        Assert.NotEmpty(dto.Token);
        Assert.True(dto.ExpiraEm > DateTime.UtcNow);
    }

    [Fact]
    public void Executar_ComSenhaErrada_DeveLancarUnauthorized()
    {
        Assert.Throws<UnauthorizedAccessException>(() =>
            CriarUseCase().Executar(new LoginCommand("admin", "senha-errada"))
        );
    }

    [Fact]
    public void Executar_ComLoginErrado_DeveLancarUnauthorized()
    {
        Assert.Throws<UnauthorizedAccessException>(() =>
            CriarUseCase().Executar(new LoginCommand("outro", "Visiosys@Dev1"))
        );
    }
}
