export function AjudaPage() {
  return (
    <div>
      <div className="page-header">
        <h2>Ajuda</h2>
      </div>

      <div className="form">
        <h3>Como usar o Visiosys</h3>
        <p>
          O Visiosys organiza o acompanhamento de precatórios por cliente. O fluxo básico é:
        </p>
        <ol>
          <li>Cadastre o cliente (credor) em <strong>Clientes → Novo</strong>.</li>
          <li>Cadastre o precatório em <strong>Precatórios → Novo</strong>, vinculando ao cliente.</li>
          <li>Registre andamentos (contatos, documentos, propostas) na página de detalhe do precatório.</li>
          <li>Registre o pagamento quando o precatório for liquidado, com o valor pago e a data.</li>
        </ol>

        <h3>Documentação técnica da API</h3>
        <p>
          A documentação completa dos endpoints (formato OpenAPI/Swagger) fica em{' '}
          <a href={`${import.meta.env.BASE_URL}swagger`} target="_blank" rel="noopener noreferrer">/swagger</a>.
          O acesso exige login com as mesmas credenciais de acesso ao sistema
          (inclusive a conta de demonstração <code>user</code> / <code>user</code>).
        </p>
      </div>
    </div>
  );
}
