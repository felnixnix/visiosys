namespace Visiosys.Application.Auditoria;

public record RegistroAuditoria(
    string Acao,
    string UsuarioLogin,
    string EntidadeTipo,
    string EntidadeId,
    DateTime OcorridoEm,
    Dictionary<string, string>? Metadados = null
);
