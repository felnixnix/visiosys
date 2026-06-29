namespace Visiosys.Application.Auditoria;

public interface IAuditLogService
{
    Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default);
}
