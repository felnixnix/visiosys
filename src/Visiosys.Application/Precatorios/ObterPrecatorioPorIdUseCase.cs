using Visiosys.Domain.Precatorios;

namespace Visiosys.Application.Precatorios;

public class ObterPrecatorioPorIdUseCase(IPrecatorioRepository repository)
{
    public async Task<PrecatorioDto?> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var precatorio = await repository.ObterPorIdAsync(id, ct);
        return precatorio is null ? null : PrecatorioDto.DeEntidade(precatorio);
    }
}
