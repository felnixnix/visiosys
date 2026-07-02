using Microsoft.Extensions.Logging.Abstractions;
using Visiosys.Application.Andamentos;
using Visiosys.Application.Auditoria;
using Visiosys.Application.Tribunais;
using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;
using Visiosys.Domain.Precatorios.Queries;
using Visiosys.Worker.Workers;

namespace Visiosys.Domain.Tests.Workers;

public class ConsultaTribunaisProcessorTests
{
    private readonly FakePrecatorioConsultaRepo _consultaRepo = new();
    private readonly FakeConsultaTribunalService _tribunalService = new();
    private readonly FakeAndamentoRepoWorker _andamentoRepo = new();
    private readonly FakePrecatorioRepoWorker _precRepo = new();
    private readonly FakeAuditLogWorker _auditLog = new();

    private ConsultaTribunaisProcessor CriarProcessor() =>
        new(_consultaRepo, _tribunalService,
            new RegistrarAndamentoUseCase(_andamentoRepo, _precRepo, _auditLog),
            NullLogger<ConsultaTribunaisProcessor>.Instance);

    [Fact]
    public async Task ProcessarAsync_SemPrecatoriosAtivos_NaoDeveConsultarTribunal()
    {
        var processor = CriarProcessor();
        await processor.ProcessarAsync(CancellationToken.None);
        Assert.False(_tribunalService.FoiConsultado);
    }

    [Fact]
    public async Task ProcessarAsync_ComPrecatorioAtivo_DeveConsultarTribunal()
    {
        var prec = Precatorio.Criar("2024/001", "TJ-SP", 100_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        _consultaRepo.Adicionar(prec);
        _precRepo.Adicionar(prec);

        var processor = CriarProcessor();
        await processor.ProcessarAsync(CancellationToken.None);

        Assert.True(_tribunalService.FoiConsultado);
    }

    [Fact]
    public async Task ProcessarAsync_ComAndamentosRetornados_DeveRegistrarAndamentos()
    {
        var prec = Precatorio.Criar("2024/002", "TJ-RJ", 50_000m, EsferaPrecatorio.Municipal, NaturezaPrecatorio.Comum);
        _consultaRepo.Adicionar(prec);
        _precRepo.Adicionar(prec);
        _tribunalService.AdicionarResposta(prec.Numero,
            new AndamentoTribunalDto("Certidão publicada no DJE.", DateTime.UtcNow));

        var processor = CriarProcessor();
        await processor.ProcessarAsync(CancellationToken.None);

        Assert.Single(_andamentoRepo.Registrados);
        Assert.Contains("Certidão publicada", _andamentoRepo.Registrados[0].Descricao);
    }

    [Fact]
    public async Task ProcessarAsync_ComErroNaTribunal_NaoDevePropagar()
    {
        var prec = Precatorio.Criar("2024/003", "TJ-MG", 80_000m, EsferaPrecatorio.Estadual, NaturezaPrecatorio.Alimentar);
        _consultaRepo.Adicionar(prec);
        _precRepo.Adicionar(prec);
        _tribunalService.SimularErro = true;

        var processor = CriarProcessor();

        // Não deve lançar exceção — erros de consulta são swallowed
        var exception = await Record.ExceptionAsync(() => processor.ProcessarAsync(CancellationToken.None));
        Assert.Null(exception);
    }
}

internal sealed class FakePrecatorioConsultaRepo : IPrecatorioConsultaRepository
{
    private readonly List<Precatorio> _store = [];
    public void Adicionar(Precatorio p) => _store.Add(p);
    public Task<IReadOnlyList<Precatorio>> ListarAtivosAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Precatorio>>(_store.ToList());
    public Task<IReadOnlyList<Precatorio>> ListarAsync(FiltroPrecatorios filtro, int skip = 0, int take = 50, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Precatorio>>(_store.Skip(skip).Take(take).ToList());
    public Task<int> ContarAsync(FiltroPrecatorios filtro, CancellationToken ct = default)
        => Task.FromResult(_store.Count);
}

internal sealed class FakeConsultaTribunalService : IConsultaTribunalService
{
    public bool FoiConsultado { get; private set; }
    public bool SimularErro { get; set; }
    private readonly Dictionary<string, List<AndamentoTribunalDto>> _respostas = [];

    public void AdicionarResposta(string numero, AndamentoTribunalDto andamento)
    {
        if (!_respostas.ContainsKey(numero)) _respostas[numero] = [];
        _respostas[numero].Add(andamento);
    }

    public Task<IReadOnlyList<AndamentoTribunalDto>> ConsultarAndamentosAsync(string numeroPrecatorio, CancellationToken ct = default)
    {
        if (SimularErro) throw new HttpRequestException("Tribunal indisponível.");
        FoiConsultado = true;
        var result = _respostas.TryGetValue(numeroPrecatorio, out var lista)
            ? (IReadOnlyList<AndamentoTribunalDto>)lista
            : [];
        return Task.FromResult(result);
    }
}

internal sealed class FakeAndamentoRepoWorker : IAndamentoRepository
{
    public List<Andamento> Registrados { get; } = [];
    public Task<IReadOnlyList<Andamento>> ListarPorPrecatorioAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Andamento>>([]);
    public Task AdicionarAsync(Andamento a, CancellationToken ct = default) { Registrados.Add(a); return Task.CompletedTask; }
    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakePrecatorioRepoWorker : IPrecatorioRepository
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

internal sealed class FakeAuditLogWorker : IAuditLogService
{
    public Task RegistrarAsync(RegistroAuditoria r, CancellationToken ct = default) => Task.CompletedTask;
}
