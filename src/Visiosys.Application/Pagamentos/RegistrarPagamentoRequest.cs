namespace Visiosys.Application.Pagamentos;

public record RegistrarPagamentoRequest(decimal ValorPago, DateTime? PagoEm = null);
