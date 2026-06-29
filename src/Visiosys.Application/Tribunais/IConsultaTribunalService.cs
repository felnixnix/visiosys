namespace Visiosys.Application.Tribunais;

public interface IConsultaTribunalService
{
    Task<IReadOnlyList<AndamentoTribunalDto>> ConsultarAndamentosAsync(
        string numeroPrecatorio, CancellationToken ct = default);
}
