using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Visiosys.Worker.Workers;

public class ConsultaTribunalWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ConsultaTribunalWorker> logger) : BackgroundService
{
    // Intervalo padrão de 30 min; substituível via appsettings Worker:IntervalMinutos
    private TimeSpan Intervalo => TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ConsultaTribunalWorker iniciado. Intervalo: {Intervalo}.", Intervalo);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<ConsultaTribunaisProcessor>();
                await processor.ProcessarAsync(stoppingToken);
            }

            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("ConsultaTribunalWorker encerrado.");
    }
}
