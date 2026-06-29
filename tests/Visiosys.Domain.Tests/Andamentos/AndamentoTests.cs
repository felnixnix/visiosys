using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Andamentos.Enums;

namespace Visiosys.Domain.Tests.Andamentos;

public class AndamentoTests
{
    private static readonly Guid PrecId = Guid.NewGuid();

    [Fact]
    public void Registrar_ComDadosValidos_DeveCriarAndamento()
    {
        var a = Andamento.Registrar(PrecId, "Documento de procuração recebido.", TipoAndamento.DocumentoRecebido, "admin");

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.Equal(PrecId, a.PrecatorioId);
        Assert.Equal(TipoAndamento.DocumentoRecebido, a.Tipo);
        Assert.True(a.OcorridoEm <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Registrar_ComDescricaoInvalida_DeveLancarExcecao(string? descricao)
    {
        Assert.Throws<ArgumentException>(() =>
            Andamento.Registrar(PrecId, descricao!, TipoAndamento.ObservacaoInterna, "admin"));
    }

    [Fact]
    public void Registrar_ComPrecatorioIdVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Andamento.Registrar(Guid.Empty, "Observação.", TipoAndamento.ObservacaoInterna, "admin"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Registrar_ComLoginInvalido_DeveLancarExcecao(string? login)
    {
        Assert.Throws<ArgumentException>(() =>
            Andamento.Registrar(PrecId, "Observação.", TipoAndamento.ObservacaoInterna, login!));
    }
}
