using Visiosys.Application.Documentos;
using Visiosys.Domain.Documentos;
using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Domain.Tests.Documentos;

public class UploadDocumentoUseCaseTests
{
    private readonly FakeDocumentoRepositoryDoc _repository = new();
    private readonly FakeArmazenamentoServiceDoc _armazenamento = new();
    private readonly UploadDocumentoUseCase _useCase;

    public UploadDocumentoUseCaseTests()
    {
        _useCase = new UploadDocumentoUseCase(_repository, _armazenamento);
    }

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_DeveRetornarDocumentoDto()
    {
        var command = new UploadDocumentoCommand(
            NomeOriginal: "procuracao.pdf",
            Tipo: TipoDocumento.Procuracao,
            Conteudo: new MemoryStream([0x25, 0x50, 0x44, 0x46]),
            TamanhoBytes: 4,
            ContentType: "application/pdf",
            EnviadoPorLogin: "admin",
            PrecatorioId: Guid.NewGuid(),
            ClienteId: null
        );

        var dto = await _useCase.ExecutarAsync(command);

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("procuracao.pdf", dto.NomeOriginal);
        Assert.True(_armazenamento.UploadFoiChamado);
    }

    [Fact]
    public async Task ExecutarAsync_ComContentTypeInvalido_DeveLancarExcecao()
    {
        var command = new UploadDocumentoCommand(
            NomeOriginal: "imagem.png",
            Tipo: TipoDocumento.Outro,
            Conteudo: new MemoryStream([0x89]),
            TamanhoBytes: 1,
            ContentType: "image/png",
            EnviadoPorLogin: "admin",
            PrecatorioId: Guid.NewGuid(),
            ClienteId: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecutarAsync(command));
    }
}

internal sealed class FakeDocumentoRepositoryDoc : IDocumentoRepository
{
    private readonly List<Documento> _store = [];

    public Task<Documento?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(d => d.Id == id));

    public Task AdicionarAsync(Documento documento, CancellationToken ct = default)
    {
        _store.Add(documento);
        return Task.CompletedTask;
    }

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class FakeArmazenamentoServiceDoc : IArmazenamentoService
{
    public bool UploadFoiChamado { get; private set; }

    public Task<UploadResultado> FazerUploadAsync(
        Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        UploadFoiChamado = true;
        var chave = $"documentos/{Guid.NewGuid()}/{nomeArquivo}";
        return Task.FromResult(new UploadResultado(chave, $"https://fake-s3/{chave}"));
    }
}
