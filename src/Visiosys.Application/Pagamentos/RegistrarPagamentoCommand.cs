namespace Visiosys.Application.Pagamentos;

public record RegistrarPagamentoCommand(
    Guid PrecatorioId,
    decimal ValorPago,
    string RegistradoPorLogin,
    DateTime? PagoEm = null
);
