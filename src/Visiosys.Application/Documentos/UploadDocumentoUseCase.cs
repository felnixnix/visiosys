using Visiosys.Domain.Documentos;

namespace Visiosys.Application.Documentos;

public class UploadDocumentoUseCase(IDocumentoRepository repository, IArmazenamentoService armazenamento)
{
    private static readonly HashSet<string> ContentTypesPermitidos =
        ["application/pdf"];

    public async Task<DocumentoDto> ExecutarAsync(UploadDocumentoCommand command, CancellationToken ct = default)
    {
        if (!ContentTypesPermitidos.Contains(command.ContentType))
            throw new InvalidOperationException("Apenas arquivos PDF são aceitos (application/pdf).");

        var resultado = await armazenamento.FazerUploadAsync(
            command.Conteudo, command.NomeOriginal, command.ContentType, ct);

        var documento = Documento.Criar(
            command.NomeOriginal,
            command.Tipo,
            resultado.Chave,
            resultado.Url,
            command.TamanhoBytes,
            command.ContentType,
            command.EnviadoPorLogin,
            command.PrecatorioId,
            command.ClienteId
        );

        await repository.AdicionarAsync(documento, ct);
        await repository.SalvarAsync(ct);

        return DocumentoDto.DeEntidade(documento);
    }
}
