namespace Visiosys.Domain.Clientes;

/// <summary>
/// Critérios de busca para a listagem de clientes. Campos nulos/vazios são
/// ignorados; os presentes combinam com E (AND).
/// </summary>
public record FiltroClientes(
    string? Nome = null,
    string? Documento = null,
    string? Tipo = null,    // "PF" (11 dígitos) | "PJ" (14 dígitos)
    string? Letra = null);  // inicial do nome (A-Z)
