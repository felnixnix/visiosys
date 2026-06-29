using Visiosys.Domain.Documentos;
using Visiosys.Domain.Documentos.Enums;

namespace Visiosys.Domain.Tests.Documentos;

public class DocumentoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarDocumento()
    {
        var precId = Guid.NewGuid();

        var doc = Documento.Criar(
            nomeOriginal: "procuracao.pdf",
            tipo: TipoDocumento.Procuracao,
            chaveArmazenamento: "documentos/2026/procuracao.pdf",
            urlDownload: "https://s3.example.com/procuracao.pdf",
            tamanhoBytes: 102400,
            contentType: "application/pdf",
            enviadoPorLogin: "admin",
            precatorioId: precId);

        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal("procuracao.pdf", doc.NomeOriginal);
        Assert.Equal(TipoDocumento.Procuracao, doc.Tipo);
        Assert.Equal(precId, doc.PrecatorioId);
        Assert.Null(doc.ClienteId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNomeOriginalInvalido_DeveLancarExcecao(string? nome)
    {
        Assert.Throws<ArgumentException>(() =>
            Documento.Criar(nome!, TipoDocumento.Certidao,
                "chave/doc.pdf", "https://url.com", 1024, "application/pdf", "admin"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComChaveInvalida_DeveLancarExcecao(string? chave)
    {
        Assert.Throws<ArgumentException>(() =>
            Documento.Criar("doc.pdf", TipoDocumento.Contrato,
                chave!, "https://url.com", 1024, "application/pdf", "admin"));
    }

    [Fact]
    public void Criar_ComTamanhoZeroOuNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Documento.Criar("doc.pdf", TipoDocumento.Peticao,
                "chave/doc.pdf", "https://url.com", 0, "application/pdf", "admin"));
    }

    [Fact]
    public void Criar_SemPrecatorioESemCliente_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Documento.Criar("doc.pdf", TipoDocumento.Outro,
                "chave/doc.pdf", "https://url.com", 1024, "application/pdf", "admin",
                precatorioId: null, clienteId: null));
    }

    [Fact]
    public void Criar_AssociadoSoAoCliente_DeveFuncionar()
    {
        var clienteId = Guid.NewGuid();

        var doc = Documento.Criar("identidade.pdf", TipoDocumento.Certidao,
            "docs/identidade.pdf", "https://s3.example.com/identidade.pdf",
            2048, "application/pdf", "admin", clienteId: clienteId);

        Assert.Equal(clienteId, doc.ClienteId);
        Assert.Null(doc.PrecatorioId);
    }
}
