using Visiosys.Application.Documentos;

namespace Visiosys.Infrastructure.Storage;

// Implementação local de desenvolvimento — não persiste arquivos fisicamente.
// Em produção substituir por S3ArmazenamentoService (Fase 5 / Terraform).
public class LocalArmazenamentoService : IArmazenamentoService
{
    public Task<UploadResultado> FazerUploadAsync(
        Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        var chave = $"documentos/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{nomeArquivo}";
        var url = $"http://localhost/storage/{chave}";
        return Task.FromResult(new UploadResultado(chave, url));
    }
}
