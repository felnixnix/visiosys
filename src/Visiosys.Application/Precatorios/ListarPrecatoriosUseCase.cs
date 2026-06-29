using Visiosys.Domain.Precatorios.Queries;

namespace Visiosys.Application.Precatorios;

public record PaginaDto<T>(IReadOnlyList<T> Items, int Total, int Pagina, int Tamanho);

public class ListarPrecatoriosUseCase(IPrecatorioConsultaRepository consultaRepository)
{
    public async Task<PaginaDto<PrecatorioDto>> ExecutarAsync(
        int pagina = 1, int tamanho = 20, CancellationToken ct = default)
    {
        tamanho = Math.Clamp(tamanho, 1, 100);
        var skip = (Math.Max(1, pagina) - 1) * tamanho;

        var items = await consultaRepository.ListarAsync(skip, tamanho, ct);
        var total = await consultaRepository.ContarAsync(ct);

        return new PaginaDto<PrecatorioDto>(
            items.Select(PrecatorioDto.DeEntidade).ToList(),
            total, pagina, tamanho);
    }
}
