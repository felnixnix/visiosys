using Visiosys.Domain.Clientes;
using Visiosys.Domain.Clientes.Enums;

namespace Visiosys.Application.Clientes;

public record ClienteDto(
    Guid Id,
    string Nome,
    string Documento,
    string Email,
    string? Telefone,
    string? BancoCodigo,
    string? BancoAgencia,
    string? BancoNumeroConta,
    TipoConta? BancoTipoConta,
    DateTime CriadoEm
)
{
    public static ClienteDto DeEntidade(Cliente c) => new(
        c.Id, c.Nome, c.Documento, c.Email, c.Telefone,
        c.DadosBancarios?.Banco,
        c.DadosBancarios?.Agencia,
        c.DadosBancarios?.NumeroConta,
        c.DadosBancarios?.TipoConta,
        c.CriadoEm
    );
}
