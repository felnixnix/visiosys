using Microsoft.Extensions.Configuration;
using Visiosys.Application.Documentos;

namespace Visiosys.Infrastructure.Storage;

// Implementação local de desenvolvimento — não persiste arquivos fisicamente.
// Em produção substituir por S3ArmazenamentoService (Fase 5 / Terraform).
public class LocalArmazenamentoService(IConfiguration config) : IArmazenamentoService
{
    public Task<UploadResultado> FazerUploadAsync(
        Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        var baseUrl = config["Storage:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000/storage";
        var chave = $"documentos/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{nomeArquivo}";
        var url = $"{baseUrl}/{chave}";
        return Task.FromResult(new UploadResultado(chave, url));
    }
}
