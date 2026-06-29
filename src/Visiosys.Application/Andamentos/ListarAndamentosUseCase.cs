using Visiosys.Domain.Andamentos;

namespace Visiosys.Application.Andamentos;

public class ListarAndamentosUseCase(IAndamentoRepository repository)
{
    public async Task<IReadOnlyList<AndamentoDto>> ExecutarAsync(Guid precatorioId, CancellationToken ct = default)
    {
        var andamentos = await repository.ListarPorPrecatorioAsync(precatorioId, ct);
        return andamentos.Select(AndamentoDto.DeEntidade).ToList();
    }
}
