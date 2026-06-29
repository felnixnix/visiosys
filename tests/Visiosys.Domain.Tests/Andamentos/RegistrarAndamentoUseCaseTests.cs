using Visiosys.Application.Andamentos;
using Visiosys.Application.Auditoria;
using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Andamentos.Enums;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Tests.Andamentos;

public class RegistrarAndamentoUseCaseTests
{
    private readonly FakePrecatorioRepositoryAnd _precRepo = new();
    private readonly FakeAndamentoRepository _andRepo = new();
    private readonly FakeAuditLogService _auditLog = new();
    private readonly RegistrarAndamentoUseCase _useCase;

    public RegistrarAndamentoUseCaseTests()
    {
        _useCase = new RegistrarAndamentoUseCase(_andRepo, _precRepo, _auditLog);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrecatorioExistente_DeveRetornarDto()
    {
        var prec = Precatorio.Criar("2024/001", "TJ-SP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        _precRepo.Adicionar(prec);

        var command = new RegistrarAndamentoCommand(
            PrecatorioId: prec.Id,
            Descricao: "Proposta enviada ao devedor.",
            Tipo: TipoAndamento.PropostaEnviada,
            RegistradoPorLogin: "admin"
        );

        var dto = await _useCase.ExecutarAsync(command);

        Assert.Equal(prec.Id, dto.PrecatorioId);
        Assert.Equal("Proposta enviada ao devedor.", dto.Descricao);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrecatorioInexistente_DeveLancarExcecao()
    {
        var command = new RegistrarAndamentoCommand(
            PrecatorioId: Guid.NewGuid(),
            Descricao: "Qualquer.",
            Tipo: TipoAndamento.ObservacaoInterna,
            RegistradoPorLogin: "admin"
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecutarAsync(command));
    }

    [Fact]
    public async Task ExecutarAsync_ComSucesso_DeveRegistrarAuditoria()
    {
        var prec = Precatorio.Criar("2024/002", "TJ-SP", 50_000m, EsferaPrecatorio.Municipal, NaturezaPrecatorio.Comum);
        _precRepo.Adicionar(prec);

        var command = new RegistrarAndamentoCommand(prec.Id, "Contato realizado.", TipoAndamento.ContatoRealizado, "operador");

        await _useCase.ExecutarAsync(command);

        Assert.True(_auditLog.RegistroChamado);
    }
}

internal sealed class FakePrecatorioRepositoryAnd : IPrecatorioRepository
{
    private readonly List<Precatorio> _store = [];

    public void Adicionar(Precatorio p) => _store.Add(p);

    public Task<Precatorio?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExisteNumeroAsync(string numero, CancellationToken ct = default)
        => Task.FromResult(_store.Any(p => p.Numero == numero));

    public Task AdicionarAsync(Precatorio precatorio, CancellationToken ct = default)
    {
        _store.Add(precatorio);
        return Task.CompletedTask;
    }

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakeAndamentoRepository : IAndamentoRepository
{
    private readonly List<Andamento> _store = [];

    public Task<IReadOnlyList<Andamento>> ListarPorPrecatorioAsync(Guid precatorioId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Andamento>>(_store.Where(a => a.PrecatorioId == precatorioId).ToList());

    public Task AdicionarAsync(Andamento andamento, CancellationToken ct = default)
    {
        _store.Add(andamento);
        return Task.CompletedTask;
    }

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakeAuditLogService : IAuditLogService
{
    public bool RegistroChamado { get; private set; }

    public Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default)
    {
        RegistroChamado = true;
        return Task.CompletedTask;
    }
}
