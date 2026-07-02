using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Precatorios.Queries;

/// <summary>
/// Critérios de busca para a listagem de precatórios. Campos nulos são
/// ignorados; os presentes combinam com E (AND).
/// </summary>
public record FiltroPrecatorios(
    string? Numero = null,
    string? Tribunal = null,
    EsferaPrecatorio? Esfera = null,
    StatusPrecatorio? Status = null,
    NaturezaPrecatorio? Natureza = null);
