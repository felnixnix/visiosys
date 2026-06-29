using Microsoft.Extensions.Logging;
using Visiosys.Application.Andamentos;
using Visiosys.Application.Tribunais;
using Visiosys.Domain.Andamentos.Enums;
using Visiosys.Domain.Precatorios.Queries;

namespace Visiosys.Worker.Workers;

public class ConsultaTribunaisProcessor(
    IPrecatorioConsultaRepository consultaRepository,
    IConsultaTribunalService tribunalService,
    RegistrarAndamentoUseCase registrarAndamento,
    ILogger<ConsultaTribunaisProcessor> logger)
{
    public async Task ProcessarAsync(CancellationToken ct)
    {
        var precatorios = await consultaRepository.ListarAtivosAsync(ct);

        if (precatorios.Count == 0)
        {
            logger.LogDebug("Nenhum precatório ativo para consulta.");
            return;
        }

        logger.LogInformation("Iniciando consulta a tribunais para {Total} precatório(s).", precatorios.Count);

        foreach (var precatorio in precatorios)
        {
            try
            {
                var andamentos = await tribunalService.ConsultarAndamentosAsync(precatorio.Numero, ct);

                foreach (var a in andamentos)
                {
                    var descricao = $"[Tribunal] {a.Descricao} (data: {a.Data:dd/MM/yyyy})";
                    await registrarAndamento.ExecutarAsync(new RegistrarAndamentoCommand(
                        PrecatorioId: precatorio.Id,
                        Descricao: descricao,
                        Tipo: TipoAndamento.AtualizacaoStatus,
                        RegistradoPorLogin: "worker.tribunal"), ct);
                }

                if (andamentos.Count > 0)
                    logger.LogInformation("{Total} andamento(s) registrado(s) para {Numero}.",
                        andamentos.Count, precatorio.Numero);
            }
            catch (Exception ex)
            {
                // Erro em um precatório não deve parar o processamento dos demais
                logger.LogError(ex, "Falha ao consultar tribunal para precatório {Numero}.", precatorio.Numero);
            }
        }
    }
}
