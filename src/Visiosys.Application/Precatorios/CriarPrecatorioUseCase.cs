using Visiosys.Domain.Precatorios;

namespace Visiosys.Application.Precatorios;

public class CriarPrecatorioUseCase(IPrecatorioRepository repository)
{
    public async Task<PrecatorioDto> ExecutarAsync(CriarPrecatorioCommand command, CancellationToken ct = default)
    {
        if (await repository.ExisteNumeroAsync(command.Numero, ct))
            throw new InvalidOperationException($"Já existe um precatório com o número '{command.Numero}'.");

        var precatorio = Precatorio.Criar(command.Numero, command.TribunalOrigem, command.ValorFace, command.Esfera, command.Natureza);

        await repository.AdicionarAsync(precatorio, ct);
        await repository.SalvarAsync(ct);

        return PrecatorioDto.DeEntidade(precatorio);
    }
}
