namespace Visiosys.Domain.Pagamentos;

public class Pagamento
{
    public Guid Id { get; private set; }
    public Guid PrecatorioId { get; private set; }
    public decimal ValorPago { get; private set; }
    public decimal ValorBase { get; private set; }
    public decimal PercDesagio { get; private set; }
    public string RegistradoPorLogin { get; private set; } = null!;
    public DateTime PagoEm { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Pagamento() { }

    public static Pagamento Registrar(
        Guid precatorioId,
        decimal valorPago,
        decimal valorBase,
        string registradoPorLogin,
        DateTime? pagoEm = null)
    {
        if (precatorioId == Guid.Empty)
            throw new ArgumentException("O precatório é obrigatório.", nameof(precatorioId));
        if (valorPago <= 0)
            throw new ArgumentException("O valor pago deve ser positivo.", nameof(valorPago));
        if (valorPago > valorBase)
            throw new ArgumentException(
                "O valor pago não pode ser superior ao valor base do precatório.", nameof(valorPago));

        var percDesagio = Math.Round((1m - valorPago / valorBase) * 100m, 4);

        return new Pagamento
        {
            Id = Guid.NewGuid(),
            PrecatorioId = precatorioId,
            ValorPago = valorPago,
            ValorBase = valorBase,
            PercDesagio = percDesagio,
            RegistradoPorLogin = registradoPorLogin,
            PagoEm = pagoEm ?? DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        };
    }
}
