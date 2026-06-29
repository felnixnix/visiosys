using Visiosys.Domain.Pagamentos;

namespace Visiosys.Domain.Tests.Pagamentos;

public class PagamentoTests
{
    private static readonly Guid PrecId = Guid.NewGuid();
    private const decimal ValorBase = 100_000m;

    [Fact]
    public void Registrar_ComDadosValidos_DeveCalcularDesagio()
    {
        var pagamento = Pagamento.Registrar(PrecId, valorPago: 70_000m, valorBase: ValorBase, "admin");

        Assert.NotEqual(Guid.Empty, pagamento.Id);
        Assert.Equal(PrecId, pagamento.PrecatorioId);
        Assert.Equal(70_000m, pagamento.ValorPago);
        Assert.Equal(ValorBase, pagamento.ValorBase);
        Assert.Equal(30m, pagamento.PercDesagio);
    }

    [Fact]
    public void Registrar_ComDesagioZero_DeveFuncionar()
    {
        var pagamento = Pagamento.Registrar(PrecId, valorPago: ValorBase, valorBase: ValorBase, "admin");
        Assert.Equal(0m, pagamento.PercDesagio);
    }

    [Fact]
    public void Registrar_ComValorPagoMaiorQueBase_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Pagamento.Registrar(PrecId, valorPago: 110_000m, valorBase: ValorBase, "admin"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Registrar_ComValorPagoInvalido_DeveLancarExcecao(decimal valorPago)
    {
        Assert.Throws<ArgumentException>(() =>
            Pagamento.Registrar(PrecId, valorPago, ValorBase, "admin"));
    }

    [Fact]
    public void Registrar_ComPrecatorioIdVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Pagamento.Registrar(Guid.Empty, 70_000m, ValorBase, "admin"));
    }

    [Fact]
    public void Registrar_ComDataPagamento_DeveUsarDataFornecida()
    {
        var dataPassada = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var pagamento = Pagamento.Registrar(PrecId, 80_000m, ValorBase, "admin", pagoEm: dataPassada);
        Assert.Equal(dataPassada, pagamento.PagoEm);
    }

    [Fact]
    public void Registrar_SemDataPagamento_DeveUsarUtcNow()
    {
        var antes = DateTime.UtcNow;
        var pagamento = Pagamento.Registrar(PrecId, 80_000m, ValorBase, "admin");
        Assert.True(pagamento.PagoEm >= antes);
    }

    [Theory]
    [InlineData(70_000, 100_000, 30.0000)]
    [InlineData(85_000, 100_000, 15.0000)]
    [InlineData(50_000, 200_000, 75.0000)]
    public void Registrar_DeveCalcularDesagioCorretamente(
        decimal valorPago, decimal valorBase, decimal desagioEsperado)
    {
        var pagamento = Pagamento.Registrar(PrecId, valorPago, valorBase, "admin");
        Assert.Equal(desagioEsperado, pagamento.PercDesagio);
    }
}
