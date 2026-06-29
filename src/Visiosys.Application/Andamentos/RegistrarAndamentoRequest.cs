using Visiosys.Domain.Andamentos.Enums;

namespace Visiosys.Application.Andamentos;

public record RegistrarAndamentoRequest(string Descricao, TipoAndamento Tipo);
