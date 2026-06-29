using Visiosys.Domain.Clientes;

namespace Visiosys.Application.Clientes;

public class ObterClientePorIdUseCase(IClienteRepository repository)
{
    public async Task<ClienteDto?> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await repository.ObterPorIdAsync(id, ct);
        return cliente is null ? null : ClienteDto.DeEntidade(cliente);
    }
}
