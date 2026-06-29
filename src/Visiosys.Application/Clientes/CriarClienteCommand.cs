namespace Visiosys.Application.Clientes;

public record CriarClienteCommand(
    string Nome,
    string Documento,
    string Email,
    string? Telefone
);
