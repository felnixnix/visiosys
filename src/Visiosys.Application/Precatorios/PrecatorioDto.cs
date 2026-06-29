using Visiosys.Domain.Precatorios;
using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Application.Precatorios;

public record PrecatorioDto(
    Guid Id,
    string Numero,
    string TribunalOrigem,
    decimal ValorFace,
    decimal? ValorAtualizado,
    EsferaPrecatorio Esfera,
    NaturezaPrecatorio Natureza,
    StatusPrecatorio Status,
    Guid? ClienteId,
    DateTime CriadoEm
)
{
    public static PrecatorioDto DeEntidade(Precatorio p) => new(
        p.Id, p.Numero, p.TribunalOrigem, p.ValorFace,
        p.ValorAtualizado, p.Esfera, p.Natureza, p.Status, p.ClienteId, p.CriadoEm
    );
}
