using Microsoft.Extensions.Logging;
using Visiosys.Application.Auditoria;
using Visiosys.Domain.Pagamentos;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Application.Pagamentos;

public class RegistrarPagamentoUseCase(
    IPagamentoRepository pagamentoRepository,
    IPrecatorioRepository precatorioRepository,
    IAuditLogService auditLog,
    ILogger<RegistrarPagamentoUseCase>? logger = null)
{
    public async Task<PagamentoDto> ExecutarAsync(RegistrarPagamentoCommand command, CancellationToken ct = default)
    {
        var precatorio = await precatorioRepository.ObterPorIdAsync(command.PrecatorioId, ct)
            ?? throw new KeyNotFoundException($"Precatório '{command.PrecatorioId}' não encontrado.");

        if (precatorio.Status == StatusPrecatorio.Liquidado)
            throw new InvalidOperationException("O precatório já foi liquidado.");
        if (precatorio.Status == StatusPrecatorio.Cancelado)
            throw new InvalidOperationException("O precatório está cancelado e não pode ser pago.");

        var valorBase = precatorio.ValorAtualizado ?? precatorio.ValorFace;

        var pagamento = Pagamento.Registrar(
            command.PrecatorioId,
            command.ValorPago,
            valorBase,
            command.RegistradoPorLogin,
            command.PagoEm);

        precatorio.AvancarStatus(StatusPrecatorio.Liquidado);

        await pagamentoRepository.AdicionarAsync(pagamento, ct);
        await pagamentoRepository.SalvarAsync(ct);
        await precatorioRepository.SalvarAsync(ct);

        try
        {
            await auditLog.RegistrarAsync(new RegistroAuditoria(
                Acao: "PAGAMENTO_REGISTRADO",
                UsuarioLogin: command.RegistradoPorLogin,
                EntidadeTipo: "Precatorio",
                EntidadeId: command.PrecatorioId.ToString(),
                OcorridoEm: DateTime.UtcNow,
                Metadados: new()
                {
                    ["valor_pago"]   = command.ValorPago.ToString("F2"),
                    ["perc_desagio"] = pagamento.PercDesagio.ToString("F4")
                }), ct);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Falha ao registrar auditoria para pagamento {PagamentoId}", pagamento.Id);
        }

        return PagamentoDto.DeEntidade(pagamento);
    }
}
