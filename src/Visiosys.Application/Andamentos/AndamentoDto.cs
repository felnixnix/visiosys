using Visiosys.Domain.Andamentos;
using Visiosys.Domain.Andamentos.Enums;

namespace Visiosys.Application.Andamentos;

public record AndamentoDto(
    Guid Id,
    Guid PrecatorioId,
    string Descricao,
    TipoAndamento Tipo,
    string RegistradoPorLogin,
    DateTime OcorridoEm
)
{
    public static AndamentoDto DeEntidade(Andamento a) => new(
        a.Id, a.PrecatorioId, a.Descricao, a.Tipo, a.RegistradoPorLogin, a.OcorridoEm);
}
