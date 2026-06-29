namespace Visiosys.Domain.Precatorios.Queries;

public interface IPrecatorioConsultaRepository
{
    Task<IReadOnlyList<Precatorio>> ListarAtivosAsync(CancellationToken ct = default);
}
