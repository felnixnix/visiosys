using Microsoft.EntityFrameworkCore;
using Serilog;
using Visiosys.Application.Andamentos;
using Visiosys.Application.Auditoria;
using Visiosys.Application.Tribunais;
using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Queries;
using Visiosys.Infrastructure.Auditoria;
using Visiosys.Infrastructure.Persistence;
using Visiosys.Infrastructure.Persistence.Repositories;
using Visiosys.Infrastructure.Tribunais;
using Visiosys.Worker.Workers;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((_, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    builder.Services.AddDbContext<VisiosysDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

    builder.Services.AddScoped<IPrecatorioRepository, PrecatorioRepository>();
    builder.Services.AddScoped<IPrecatorioConsultaRepository>(sp =>
        (PrecatorioRepository)sp.GetRequiredService<IPrecatorioRepository>());

    builder.Services.AddScoped<IAndamentoRepository, AndamentoRepository>();
    builder.Services.AddScoped<RegistrarAndamentoUseCase>();

    builder.Services.AddSingleton<IAuditLogService, MongoAuditLogService>();

    // HttpClient com resiliência padrão: retry exponencial + circuit breaker
    builder.Services.AddHttpClient<IConsultaTribunalService, TribunalHttpClient>()
        .AddStandardResilienceHandler();

    builder.Services.AddScoped<ConsultaTribunaisProcessor>();
    builder.Services.AddHostedService<ConsultaTribunalWorker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
