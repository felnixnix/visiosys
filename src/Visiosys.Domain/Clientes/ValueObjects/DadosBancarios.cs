using Visiosys.Domain.Clientes.Enums;

namespace Visiosys.Domain.Clientes.ValueObjects;

public record DadosBancarios
{
    public string Banco { get; }
    public string Agencia { get; }
    public string NumeroConta { get; }
    public TipoConta TipoConta { get; }

    public DadosBancarios(string banco, string agencia, string numeroConta, TipoConta tipoConta)
    {
        if (string.IsNullOrWhiteSpace(banco))
            throw new ArgumentException("O código do banco é obrigatório.", nameof(banco));
        if (string.IsNullOrWhiteSpace(agencia))
            throw new ArgumentException("A agência é obrigatória.", nameof(agencia));
        if (string.IsNullOrWhiteSpace(numeroConta))
            throw new ArgumentException("O número da conta é obrigatório.", nameof(numeroConta));

        Banco = banco.Trim();
        Agencia = agencia.Trim();
        NumeroConta = numeroConta.Trim();
        TipoConta = tipoConta;
    }
}
