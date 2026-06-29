using Visiosys.Application.Auditoria;
using Visiosys.Application.Pagamentos;
using Visiosys.Domain.Pagamentos;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Tests.Pagamentos;

public class RegistrarPagamentoUseCaseTests
{
    private readonly FakePrecatorioRepositoryPag _precRepo = new();
    private readonly FakePagamentoRepository _pagRepo = new();
    private readonly FakeAuditLogServicePag _auditLog = new();
    private readonly RegistrarPagamentoUseCase _useCase;

    public RegistrarPagamentoUseCaseTests()
    {
        _useCase = new RegistrarPagamentoUseCase(_pagRepo, _precRepo, _auditLog);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrecatorioValido_DeveRegistrarPagamentoELiquidar()
    {
        var prec = Precatorio.Criar("2024/001", "TJ-SP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        prec.AtualizarValorAtualizado(110_000m);
        _precRepo.Adicionar(prec);

        var command = new RegistrarPagamentoCommand(
            PrecatorioId: prec.Id,
            ValorPago: 77_000m,
            RegistradoPorLogin: "admin"
        );

        var dto = await _useCase.ExecutarAsync(command);

        Assert.Equal(prec.Id, dto.PrecatorioId);
        Assert.Equal(77_000m, dto.ValorPago);
        Assert.Equal(110_000m, dto.ValorBase);
        Assert.Equal(30m, dto.PercDesagio);
        Assert.Equal(StatusPrecatorio.Liquidado, prec.Status);
    }

    [Fact]
    public async Task ExecutarAsync_SemValorAtualizado_DeveUsarValorFaceComoBase()
    {
        var prec = Precatorio.Criar("2024/002", "TJ-SP", 100_000m, EsferaPrecatorio.Municipal, NaturezaPrecatorio.Comum);
        _precRepo.Adicionar(prec);

        var command = new RegistrarPagamentoCommand(prec.Id, 70_000m, "admin");

        var dto = await _useCase.ExecutarAsync(command);

        Assert.Equal(100_000m, dto.ValorBase);
        Assert.Equal(30m, dto.PercDesagio);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrecatorioInexistente_DeveLancarExcecao()
    {
        var command = new RegistrarPagamentoCommand(Guid.NewGuid(), 70_000m, "admin");
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecutarAsync(command));
    }

    [Fact]
    public async Task ExecutarAsync_ComPrecatorioJaLiquidado_DeveLancarExcecao()
    {
        var prec = Precatorio.Criar("2024/003", "TJ-SP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Comum);
        prec.AvancarStatus(StatusPrecatorio.Liquidado);
        _precRepo.Adicionar(prec);

        var command = new RegistrarPagamentoCommand(prec.Id, 70_000m, "admin");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecutarAsync(command));
    }

    [Fact]
    public async Task ExecutarAsync_ComSucesso_DeveRegistrarAuditoria()
    {
        var prec = Precatorio.Criar("2024/004", "TJ-RJ", 50_000m, EsferaPrecatorio.Municipal, NaturezaPrecatorio.Alimentar);
        _precRepo.Adicionar(prec);

        await _useCase.ExecutarAsync(new RegistrarPagamentoCommand(prec.Id, 35_000m, "operador"));

        Assert.True(_auditLog.RegistroChamado);
    }
}

internal sealed class FakePrecatorioRepositoryPag : IPrecatorioRepository
{
    private readonly List<Precatorio> _store = [];

    public void Adicionar(Precatorio p) => _store.Add(p);

    public Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default)
        => Task.FromResult(_store.Any(p => p.Numero == numero));

    public Task AdicionarAsync(Precatorio p, CancellationToken ct = default) { _store.Add(p); return Task.CompletedTask; }
    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakePagamentoRepository : IPagamentoRepository
{
    private readonly List<Pagamento> _store = [];

    public Task<IReadOnlyList<Pagamento>> ListarPorPrecatorioAsync(Guid precId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Pagamento>>(_store.Where(p => p.PrecatorioId == precId).ToList());

    public Task AdicionarAsync(Pagamento p, CancellationToken ct = default) { _store.Add(p); return Task.CompletedTask; }
    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakeAuditLogServicePag : IAuditLogService
{
    public bool RegistroChamado { get; private set; }
    public Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default)
    {
        RegistroChamado = true;
        return Task.CompletedTask;
    }
}
