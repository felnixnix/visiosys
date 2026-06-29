using Visiosys.Domain.Precatorios.Enums;

namespace Visiosys.Domain.Precatorios;

public class Precatorio
{
    public Guid Id { get; private set; }
    public string Numero { get; private set; } = null!;
    public string TribunalOrigem { get; private set; } = null!;
    public decimal ValorFace { get; private set; }
    public decimal? ValorAtualizado { get; private set; }
    public EsferaPrecatorio Esfera { get; private set; }
    public NaturezaPrecatorio Natureza { get; private set; }
    public StatusPrecatorio Status { get; private set; }
    public Guid? ClienteId { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    private Precatorio() { }

    public static Precatorio Criar(
        string numero,
        string tribunalOrigem,
        decimal valorFace,
        EsferaPrecatorio esfera,
        NaturezaPrecatorio natureza,
        Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("O número do precatório é obrigatório.", nameof(numero));

        if (string.IsNullOrWhiteSpace(tribunalOrigem))
            throw new ArgumentException("O tribunal de origem é obrigatório.", nameof(tribunalOrigem));

        if (valorFace <= 0)
            throw new ArgumentException("O valor de face deve ser positivo.", nameof(valorFace));

        return new Precatorio
        {
            Id = Guid.NewGuid(),
            Numero = numero.Trim(),
            TribunalOrigem = tribunalOrigem.Trim(),
            ValorFace = valorFace,
            Esfera = esfera,
            Natureza = natureza,
            Status = StatusPrecatorio.EmAnalise,
            ClienteId = clienteId,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };
    }

    public void AssociarCliente(Guid clienteId)
    {
        ClienteId = clienteId;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AtualizarValorAtualizado(decimal novoValor)
    {
        if (novoValor < ValorFace)
            throw new InvalidOperationException(
                "O valor atualizado não pode ser inferior ao valor de face original.");

        ValorAtualizado = novoValor;
        AtualizadoEm = DateTime.UtcNow;
    }

    public decimal CalcularProposta(decimal taxaDesagio)
    {
        if (taxaDesagio < 0 || taxaDesagio >= 1)
            throw new ArgumentOutOfRangeException(nameof(taxaDesagio),
                "A taxa de deságio deve estar entre 0 (inclusive) e 1 (exclusive).");

        if (ValorAtualizado is null)
            throw new InvalidOperationException(
                "Não é possível calcular a proposta sem o valor atualizado do precatório.");

        return ValorAtualizado.Value * (1 - taxaDesagio);
    }

    public void AvancarStatus(StatusPrecatorio novoStatus)
    {
        if (Status == StatusPrecatorio.Liquidado || Status == StatusPrecatorio.Cancelado)
            throw new InvalidOperationException(
                $"Precatório com status '{Status}' não pode ser alterado.");

        Status = novoStatus;
        AtualizadoEm = DateTime.UtcNow;
    }
}
