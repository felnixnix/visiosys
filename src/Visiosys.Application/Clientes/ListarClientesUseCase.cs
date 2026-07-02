using Visiosys.Application.Precatorios;
using Visiosys.Domain.Clientes;

namespace Visiosys.Application.Clientes;

public class ListarClientesUseCase(IClienteRepository repository)
{
    public async Task<PaginaDto<ClienteDto>> ExecutarAsync(
        FiltroClientes filtro, int pagina = 1, int tamanho = 20, CancellationToken ct = default)
    {
        tamanho = Math.Clamp(tamanho, 1, 100);
        var skip = (Math.Max(1, pagina) - 1) * tamanho;

        var clientes = await repository.ListarAsync(filtro, skip, tamanho, ct);
        var total = await repository.ContarAsync(filtro, ct);

        var items = clientes.Select(ClienteDto.DeEntidade).ToList();
        return new PaginaDto<ClienteDto>(items, total, Math.Max(1, pagina), tamanho);
    }
}
