using Visiosys.Domain.Clientes;
using Visiosys.Domain.Clientes.Enums;
using Visiosys.Domain.Clientes.ValueObjects;

namespace Visiosys.Domain.Tests.Clientes;

public class ClienteTests
{
    private const string CpfValido  = "52998224725";
    private const string CnpjValido = "11222333000181";

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarCliente()
    {
        var cliente = Cliente.Criar("João da Silva", CpfValido, "joao@email.com");

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("João da Silva", cliente.Nome);
        Assert.Equal(CpfValido, cliente.Documento);
        Assert.Equal("joao@email.com", cliente.Email);
        Assert.Null(cliente.DadosBancarios);
    }

    [Fact]
    public void Criar_ComCnpjValido_DeveCriarCliente()
    {
        var cliente = Cliente.Criar("Empresa Ltda", CnpjValido, "empresa@email.com");
        Assert.Equal(CnpjValido, cliente.Documento);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string? nome)
    {
        Assert.Throws<ArgumentException>(() =>
            Cliente.Criar(nome!, CpfValido, "email@email.com"));
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("12345678900")]
    [InlineData("abc")]
    [InlineData("")]
    public void Criar_ComDocumentoInvalido_DeveLancarExcecao(string doc)
    {
        Assert.Throws<ArgumentException>(() =>
            Cliente.Criar("Nome", doc, "email@email.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-email")]
    public void Criar_ComEmailInvalido_DeveLancarExcecao(string email)
    {
        Assert.Throws<ArgumentException>(() =>
            Cliente.Criar("Nome", CpfValido, email));
    }

    [Fact]
    public void AtualizarDadosBancarios_ComDadosValidos_DeveAtualizar()
    {
        var cliente = Cliente.Criar("João da Silva", CpfValido, "joao@email.com");
        var dados = new DadosBancarios("001", "0001", "12345-6", TipoConta.Corrente);

        cliente.AtualizarDadosBancarios(dados);

        Assert.NotNull(cliente.DadosBancarios);
        Assert.Equal("001", cliente.DadosBancarios.Banco);
        Assert.Equal(TipoConta.Corrente, cliente.DadosBancarios.TipoConta);
    }

    [Fact]
    public void Criar_DocumentoComFormatacao_DeveNormalizarParaSoDigitos()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "j@e.com");
        Assert.Equal(CpfValido, cliente.Documento);
    }
}
