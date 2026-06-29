using Visiosys.Domain.Clientes;

namespace Visiosys.Application.Clientes;

public class CriarClienteUseCase(IClienteRepository repository)
{
    public async Task<ClienteDto> ExecutarAsync(CriarClienteCommand command, CancellationToken ct = default)
    {
        if (await repository.ExisteDocumentoAsync(command.Documento, ct))
            throw new InvalidOperationException($"Já existe um cliente com o documento '{command.Documento}'.");

        var cliente = Cliente.Criar(command.Nome, command.Documento, command.Email, command.Telefone);

        await repository.AdicionarAsync(cliente, ct);
        await repository.SalvarAsync(ct);

        return ClienteDto.DeEntidade(cliente);
    }
}
