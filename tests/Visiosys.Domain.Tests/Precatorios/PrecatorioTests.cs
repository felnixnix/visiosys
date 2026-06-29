using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Tests.Precatorios;

public class PrecatorioTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarPrecatorio()
    {
        var precatorio = Precatorio.Criar(
            numero: "0001234-56.2020.8.26.0100",
            tribunalOrigem: "TJSP",
            valorFace: 500_000m,
            esfera: EsferaPrecatorio.Estadual,
            natureza: NaturezaPrecatorio.Alimentar
        );

        Assert.Equal("0001234-56.2020.8.26.0100", precatorio.Numero);
        Assert.Equal("TJSP", precatorio.TribunalOrigem);
        Assert.Equal(500_000m, precatorio.ValorFace);
        Assert.Equal(EsferaPrecatorio.Estadual, precatorio.Esfera);
        Assert.Equal(NaturezaPrecatorio.Alimentar, precatorio.Natureza);
        Assert.Equal(StatusPrecatorio.EmAnalise, precatorio.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNumeroInvalido_DeveLancarExcecao(string? numero)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Precatorio.Criar(numero!, "TJSP", 100_000m, EsferaPrecatorio.Federal, NaturezaPrecatorio.Comum)
        );
        Assert.Contains("número", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Criar_ComValorFaceZeroOuNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 0m, EsferaPrecatorio.Federal, NaturezaPrecatorio.Comum)
        );
    }

    [Fact]
    public void AtualizarValorAtualizado_ComValorPositivo_DeveAtualizar()
    {
        var precatorio = Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 500_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);

        precatorio.AtualizarValorAtualizado(520_000m);

        Assert.Equal(520_000m, precatorio.ValorAtualizado);
    }

    [Fact]
    public void AtualizarValorAtualizado_ComValorInferiorAoFace_DeveLancarExcecao()
    {
        var precatorio = Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 500_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);

        Assert.Throws<InvalidOperationException>(() =>
            precatorio.AtualizarValorAtualizado(100_000m)
        );
    }

    [Fact]
    public void CalcularDeságio_ComTaxaValida_DeveRetornarValorCorreto()
    {
        var precatorio = Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 500_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        precatorio.AtualizarValorAtualizado(500_000m);

        var proposta = precatorio.CalcularProposta(taxaDesagio: 0.30m);

        Assert.Equal(350_000m, proposta);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void CalcularDeságio_ComTaxaForaDosLimites_DeveLancarExcecao(double taxa)
    {
        var precatorio = Precatorio.Criar("0001234-56.2020.8.26.0100", "TJSP", 500_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        precatorio.AtualizarValorAtualizado(500_000m);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            precatorio.CalcularProposta((decimal)taxa)
        );
    }
}
