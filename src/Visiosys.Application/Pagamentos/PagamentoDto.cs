using Visiosys.Domain.Pagamentos;

namespace Visiosys.Application.Pagamentos;

public record PagamentoDto(
    Guid Id,
    Guid PrecatorioId,
    decimal ValorPago,
    decimal ValorBase,
    decimal PercDesagio,
    string RegistradoPorLogin,
    DateTime PagoEm,
    DateTime CriadoEm
)
{
    public static PagamentoDto DeEntidade(Pagamento p) => new(
        p.Id, p.PrecatorioId, p.ValorPago, p.ValorBase,
        p.PercDesagio, p.RegistradoPorLogin, p.PagoEm, p.CriadoEm);
}
