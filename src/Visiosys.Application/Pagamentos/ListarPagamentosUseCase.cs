using Visiosys.Domain.Pagamentos;

namespace Visiosys.Application.Pagamentos;

public class ListarPagamentosUseCase(IPagamentoRepository repository)
{
    public async Task<IReadOnlyList<PagamentoDto>> ExecutarAsync(Guid precatorioId, CancellationToken ct = default)
    {
        var pagamentos = await repository.ListarPorPrecatorioAsync(precatorioId, ct);
        return pagamentos.Select(PagamentoDto.DeEntidade).ToList();
    }
}
