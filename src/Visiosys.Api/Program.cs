using Microsoft.EntityFrameworkCore;
using Visiosys.Application.Precatorios;
using Visiosys.Domain.Precatorios;
using Visiosys.Infrastructure.Persistence;
using Visiosys.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<VisiosysDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IPrecatorioRepository, PrecatorioRepository>();
builder.Services.AddScoped<CriarPrecatorioUseCase>();
builder.Services.AddScoped<ObterPrecatorioPorIdUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
