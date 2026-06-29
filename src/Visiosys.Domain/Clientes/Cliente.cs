using System.Text.RegularExpressions;
using Visiosys.Domain.Clientes.ValueObjects;

namespace Visiosys.Domain.Clientes;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Documento { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public DadosBancarios? DadosBancarios { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    private Cliente() { }

    public static Cliente Criar(string nome, string documento, string email, string? telefone = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));

        var docDigitos = Regex.Replace(documento ?? "", @"\D", "");
        if (!DocumentoValido(docDigitos))
            throw new ArgumentException("CPF ou CNPJ inválido.", nameof(documento));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("E-mail inválido.", nameof(email));

        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Documento = docDigitos,
            Email = email.Trim(),
            Telefone = telefone?.Trim(),
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };
    }

    public void AtualizarDadosBancarios(DadosBancarios dados)
    {
        DadosBancarios = dados;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AtualizarContato(string email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("E-mail inválido.", nameof(email));

        Email = email.Trim();
        Telefone = telefone?.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    private static bool DocumentoValido(string doc)
    {
        if (doc.Length == 11) return ValidarCpf(doc);
        if (doc.Length == 14) return ValidarCnpj(doc);
        return false;
    }

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;

        var soma = 0;
        for (var i = 0; i < 9; i++) soma += (cpf[i] - '0') * (10 - i);
        var d1 = soma % 11 < 2 ? 0 : 11 - soma % 11;

        soma = 0;
        for (var i = 0; i < 10; i++) soma += (cpf[i] - '0') * (11 - i);
        var d2 = soma % 11 < 2 ? 0 : 11 - soma % 11;

        return cpf[9] - '0' == d1 && cpf[10] - '0' == d2;
    }

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;

        int[] p1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] p2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma = p1.Select((w, i) => w * (cnpj[i] - '0')).Sum();
        var d1 = soma % 11 < 2 ? 0 : 11 - soma % 11;

        soma = p2.Select((w, i) => w * (cnpj[i] - '0')).Sum();
        var d2 = soma % 11 < 2 ? 0 : 11 - soma % 11;

        return cnpj[12] - '0' == d1 && cnpj[13] - '0' == d2;
    }
}
