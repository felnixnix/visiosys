using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Visiosys.Application.Documentos;

namespace Visiosys.Infrastructure.Storage;

public class S3ArmazenamentoService(IAmazonS3 s3, IConfiguration config) : IArmazenamentoService
{
    public async Task<UploadResultado> FazerUploadAsync(
        Stream conteudo, string nomeArquivo, string contentType, CancellationToken ct = default)
    {
        var bucket = config["Storage:S3Bucket"]
            ?? throw new InvalidOperationException("Storage:S3Bucket não configurado.");

        var chave = $"documentos/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{nomeArquivo}";

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName  = bucket,
            Key         = chave,
            InputStream = conteudo,
            ContentType = contentType,
        }, ct);

        // URL pré-assinada com expiração de 1 hora para download seguro
        var url = await s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key        = chave,
            Expires    = DateTime.UtcNow.AddHours(1),
            Verb       = HttpVerb.GET,
        });

        return new UploadResultado(chave, url);
    }
}
