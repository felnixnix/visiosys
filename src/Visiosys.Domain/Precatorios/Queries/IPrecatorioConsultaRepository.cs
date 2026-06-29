namespace Visiosys.Domain.Precatorios.Queries;

public interface IPrecatorioConsultaRepository
{
    Task<IReadOnlyList<Precatorio>> ListarAtivosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Precatorio>> ListarAsync(int skip = 0, int take = 50, CancellationToken ct = default);
    Task<int> ContarAsync(CancellationToken ct = default);
}
