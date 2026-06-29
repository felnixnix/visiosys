using Visiosys.Domain.Andamentos.Enums;

namespace Visiosys.Domain.Andamentos;

public class Andamento
{
    public Guid Id { get; private set; }
    public Guid PrecatorioId { get; private set; }
    public string Descricao { get; private set; } = null!;
    public TipoAndamento Tipo { get; private set; }
    public string RegistradoPorLogin { get; private set; } = null!;
    public DateTime OcorridoEm { get; private set; }

    private Andamento() { }

    public static Andamento Registrar(
        Guid precatorioId,
        string descricao,
        TipoAndamento tipo,
        string registradoPorLogin)
    {
        if (precatorioId == Guid.Empty)
            throw new ArgumentException("O precatório é obrigatório.", nameof(precatorioId));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        if (string.IsNullOrWhiteSpace(registradoPorLogin))
            throw new ArgumentException("O login do responsável é obrigatório.", nameof(registradoPorLogin));

        return new Andamento
        {
            Id = Guid.NewGuid(),
            PrecatorioId = precatorioId,
            Descricao = descricao.Trim(),
            Tipo = tipo,
            RegistradoPorLogin = registradoPorLogin,
            OcorridoEm = DateTime.UtcNow
        };
    }
}
