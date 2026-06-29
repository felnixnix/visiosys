using Microsoft.Extensions.Logging;
using Visiosys.Application.Auditoria;
using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Precatorios;

namespace Visiosys.Application.Andamentos;

public class RegistrarAndamentoUseCase(
    IAndamentoRepository repository,
    IPrecatorioRepository precatorioRepository,
    IAuditLogService auditLog,
    ILogger<RegistrarAndamentoUseCase>? logger = null)
{
    public async Task<AndamentoDto> ExecutarAsync(RegistrarAndamentoCommand command, CancellationToken ct = default)
    {
        var precatorio = await precatorioRepository.ObterPorIdAsync(command.PrecatorioId, ct);
        if (precatorio is null)
            throw new KeyNotFoundException($"Precatório '{command.PrecatorioId}' não encontrado.");

        var andamento = Andamento.Registrar(
            command.PrecatorioId, command.Descricao, command.Tipo, command.RegistradoPorLogin);

        await repository.AdicionarAsync(andamento, ct);
        await repository.SalvarAsync(ct);

        try
        {
            await auditLog.RegistrarAsync(new RegistroAuditoria(
                Acao: "ANDAMENTO_REGISTRADO",
                UsuarioLogin: command.RegistradoPorLogin,
                EntidadeTipo: "Precatorio",
                EntidadeId: command.PrecatorioId.ToString(),
                OcorridoEm: DateTime.UtcNow,
                Metadados: new() { ["tipo"] = command.Tipo.ToString() }), ct);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Falha ao registrar auditoria para andamento {AndamentoId}", andamento.Id);
        }

        return AndamentoDto.DeEntidade(andamento);
    }
}
