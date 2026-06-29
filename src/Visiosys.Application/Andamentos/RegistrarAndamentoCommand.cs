using Visiosys.Domain.Andamentos.Enums;

namespace Visiosys.Application.Andamentos;

public record RegistrarAndamentoCommand(
    Guid PrecatorioId,
    string Descricao,
    TipoAndamento Tipo,
    string RegistradoPorLogin
);
