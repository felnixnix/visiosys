using Visiosys.Application.Precatorios;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Tests.Application;

public class CriarPrecatorioUseCaseTests
{
    private readonly FakePrecatorioRepository _repo = new();
    private readonly CriarPrecatorioUseCase _useCase;

    public CriarPrecatorioUseCaseTests()
    {
        _useCase = new CriarPrecatorioUseCase(_repo);
    }

    [Fact]
    public async Task Executar_ComDadosValidos_DevePersistirERetornarDto()
    {
        var command = new CriarPrecatorioCommand(
            "0001234-56.2020.8.26.0100", "TJSP", 500_000m,
            EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar
        );

        var dto = await _useCase.ExecutarAsync(command);

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("0001234-56.2020.8.26.0100", dto.Numero);
        Assert.Equal(500_000m, dto.ValorFace);
        Assert.Equal(StatusPrecatorio.EmAnalise, dto.Status);
        Assert.Single(_repo.Precatorios);
    }

    [Fact]
    public async Task Executar_ComNumeroJaExistente_DeveLancarExcecao()
    {
        var command = new CriarPrecatorioCommand(
            "NUMERO-DUPLICADO", "TJSP", 100_000m,
            EsferaPrecatorio.Estadual, NaturezaPrecatorio.Comum
        );
        await _useCase.ExecutarAsync(command);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecutarAsync(command)
        );
    }
}

public class ObterPrecatorioPorIdUseCaseTests
{
    private readonly FakePrecatorioRepository _repo = new();
    private readonly ObterPrecatorioPorIdUseCase _useCase;

    public ObterPrecatorioPorIdUseCaseTests()
    {
        _useCase = new ObterPrecatorioPorIdUseCase(_repo);
    }

    [Fact]
    public async Task Executar_ComIdExistente_DeveRetornarDto()
    {
        var precatorio = Precatorio.Criar("NUM-001", "STJ", 200_000m, EsferaPrecatorio.Federal, NaturezaPrecatorio.Comum);
        await _repo.AdicionarAsync(precatorio);
        await _repo.SalvarAsync();

        var dto = await _useCase.ExecutarAsync(precatorio.Id);

        Assert.NotNull(dto);
        Assert.Equal("NUM-001", dto.Numero);
    }

    [Fact]
    public async Task Executar_ComIdInexistente_DeveRetornarNull()
    {
        var dto = await _useCase.ExecutarAsync(Guid.NewGuid());

        Assert.Null(dto);
    }
}

public class FakePrecatorioRepository : IPrecatorioRepository
{
    public List<Precatorio> Precatorios { get; } = [];

    public Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Precatorios.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default)
        => Task.FromResult(Precatorios.Any(p => p.Numero == numero));

    public Task AdicionarAsync(Precatorio precatorio, CancellationToken ct = default)
    {
        Precatorios.Add(precatorio);
        return Task.CompletedTask;
    }

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}
