using Amazon;
using Amazon.S3;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Visiosys.Application.Andamentos;
using Visiosys.Application.Auth;
using Visiosys.Application.Auditoria;
using Visiosys.Application.Pagamentos;
using Visiosys.Application.Clientes;
using Visiosys.Application.Documentos;
using Visiosys.Application.Precatorios;
using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Clientes;
using Visiosys.Domain.Pagamentos;
using Visiosys.Domain.Precatorios.Queries;
using Visiosys.Domain.Documentos;
using Visiosys.Domain.Precatorios;
using Visiosys.Infrastructure.Auditoria;
using Visiosys.Infrastructure.Persistence;
using Visiosys.Infrastructure.Persistence.Repositories;
using Visiosys.Infrastructure.Storage;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers()
        .AddJsonOptions(o =>
            // Enums trafegam como strings (ex: "ObservacaoInterna", "EmAnalise"),
            // alinhado ao frontend. Aceita também inteiros na desserialização.
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Visiosys API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                []
            }
        });
    });

    // Banco de dados
    builder.Services.AddDbContext<VisiosysDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

    // Repositórios e casos de uso
    builder.Services.AddScoped<IPrecatorioRepository, PrecatorioRepository>();
    builder.Services.AddScoped<IPrecatorioConsultaRepository>(sp =>
        (PrecatorioRepository)sp.GetRequiredService<IPrecatorioRepository>());
    builder.Services.AddScoped<CriarPrecatorioUseCase>();
    builder.Services.AddScoped<ObterPrecatorioPorIdUseCase>();
    builder.Services.AddScoped<ListarPrecatoriosUseCase>();
    builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
    builder.Services.AddScoped<CriarClienteUseCase>();
    builder.Services.AddScoped<ObterClientePorIdUseCase>();
    builder.Services.AddScoped<ListarClientesUseCase>();
    builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
    // Em produção, Storage:S3Bucket deve estar configurado via env var.
    // Em dev local, usa stub que gera chave/URL sem subir arquivo.
    // Em produção, Storage:S3Bucket deve estar configurado via env var.
    // EC2 usa IAM Role — sem credenciais explícitas no código.
    // Em dev local, usa stub que gera chave/URL sem subir arquivo.
    if (builder.Configuration["Storage:S3Bucket"] is { Length: > 0 })
    {
        builder.Services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(RegionEndpoint.SAEast1));
        builder.Services.AddScoped<IArmazenamentoService, S3ArmazenamentoService>();
    }
    else
    {
        builder.Services.AddScoped<IArmazenamentoService, LocalArmazenamentoService>();
    }
    builder.Services.AddScoped<UploadDocumentoUseCase>();
    builder.Services.AddScoped<ObterDocumentoPorIdUseCase>();
    builder.Services.AddScoped<ListarDocumentosUseCase>();
    builder.Services.AddScoped<IAndamentoRepository, AndamentoRepository>();
    builder.Services.AddScoped<RegistrarAndamentoUseCase>();
    builder.Services.AddScoped<ListarAndamentosUseCase>();
    builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
    builder.Services.AddScoped<RegistrarPagamentoUseCase>();
    builder.Services.AddScoped<ListarPagamentosUseCase>();
    builder.Services.AddSingleton<IAuditLogService, MongoAuditLogService>();
    builder.Services.AddScoped<GerarTokenUseCase>();

    // Autenticação JWT (RNF09)
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidAudience = builder.Configuration["Jwt:Emissor"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Chave"]!))
        });
    builder.Services.AddAuthorization();

    // Rate Limiting nativo — protege o endpoint de login contra força bruta (RNF17)
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", o =>
        {
            o.PermitLimit = 5;
            o.Window = TimeSpan.FromMinutes(1);
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // Health Checks (RNF13)
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres");

    var app = builder.Build();

    // Path base opcional (ex: "/visiosys" em produção, atrás de um domínio
    // compartilhado — ver ADR-023). Vazio em desenvolvimento: comportamento
    // na raiz "/" não muda.
    var pathBase = app.Configuration["PathBase"];
    if (!string.IsNullOrEmpty(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    // Aplica migrations pendentes no startup (deploy single-instance).
    // Idempotente: migrations ja aplicadas sao ignoradas.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<VisiosysDbContext>();
        db.Database.Migrate();
    }

    app.UseSerilogRequestLogging();

    // Swagger fica disponível em todos os ambientes (RNF10), mas em produção
    // exige Basic Auth com as mesmas credenciais administrativas do login
    // (ver ADR-022) — evita expor o contrato da API publicamente sem proteção.
    app.Use(async (ctx, next) =>
    {
        if (app.Environment.IsDevelopment() || !ctx.Request.Path.StartsWithSegments("/swagger"))
        {
            await next();
            return;
        }

        var header = ctx.Request.Headers.Authorization.ToString();
        if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var credenciais = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..])).Split(':', 2);
            if (credenciais.Length == 2
                && credenciais[0] == app.Configuration["Auth:Login"]
                && credenciais[1] == app.Configuration["Auth:Senha"])
            {
                await next();
                return;
            }
        }

        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"Visiosys Swagger\"";
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
    });
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Fallback para React Router — rotas client-side como /precatorios/novo
    // ao receber F5 retornam index.html em vez de 404
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
