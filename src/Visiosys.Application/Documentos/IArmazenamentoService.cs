namespace Visiosys.Application.Documentos;

public record UploadResultado(string Chave, string Url);

public interface IArmazenamentoService
{
    Task<UploadResultado> FazerUploadAsync(
        Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default);
}
