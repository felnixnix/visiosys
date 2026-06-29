using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Application.Precatorios;

public record CriarPrecatorioCommand(
    string Numero,
    string TribunalOrigem,
    decimal ValorFace,
    EsferaPrecatorio Esfera,
    NaturezaPrecatorio Natureza,
    Guid? ClienteId = null
);
