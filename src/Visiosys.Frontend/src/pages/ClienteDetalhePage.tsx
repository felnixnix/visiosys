import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { clientesApi } from '../api/clientes';
import type { ClienteDto } from '../types';

function fmtDoc(doc: string) {
  if (doc.length === 11)
    return doc.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  return doc.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
}

function fmtData(iso: string) {
  return new Date(iso).toLocaleDateString('pt-BR');
}

const TIPO_CONTA: Record<string, string> = {
  Corrente: 'Conta Corrente',
  Poupanca: 'Poupança',
};

export function ClienteDetalhePage() {
  const { id } = useParams<{ id: string }>();
  const [cliente, setCliente] = useState<ClienteDto | null>(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    if (!id) return;
    clientesApi.obterPorId(id)
      .then(setCliente)
      .catch(() => setErro('Cliente não encontrado.'));
  }, [id]);

  if (erro) {
    return (
      <div>
        <p className="erro">{erro}</p>
        <Link to="/clientes">← Voltar</Link>
      </div>
    );
  }

  if (!cliente) return <p className="carregando">Carregando…</p>;

  const temDadosBancarios =
    cliente.bancoCodigo || cliente.bancoAgencia || cliente.bancoNumeroConta;

  return (
    <div>
      <div className="page-header">
        <h2>{cliente.nome}</h2>
        <Link to="/clientes" className="btn-secondary">← Clientes</Link>
      </div>

      <section style={{ marginBottom: '2rem' }}>
        <h3 style={{ marginBottom: '.75rem', fontSize: '1rem', color: '#555' }}>Dados pessoais</h3>
        <dl className="info-grid">
          <dt>CPF / CNPJ</dt>
          <dd style={{ fontFamily: 'monospace' }}>{fmtDoc(cliente.documento)}</dd>

          <dt>Email</dt>
          <dd>{cliente.email}</dd>

          <dt>Telefone</dt>
          <dd>{cliente.telefone ?? '—'}</dd>

          <dt>Cadastrado em</dt>
          <dd>{fmtData(cliente.criadoEm)}</dd>
        </dl>
      </section>

      {temDadosBancarios && (
        <section>
          <h3 style={{ marginBottom: '.75rem', fontSize: '1rem', color: '#555' }}>Dados bancários</h3>
          <dl className="info-grid">
            {cliente.bancoCodigo && (
              <>
                <dt>Banco</dt>
                <dd>{cliente.bancoCodigo}</dd>
              </>
            )}
            {cliente.bancoAgencia && (
              <>
                <dt>Agência</dt>
                <dd>{cliente.bancoAgencia}</dd>
              </>
            )}
            {cliente.bancoNumeroConta && (
              <>
                <dt>Conta</dt>
                <dd>{cliente.bancoNumeroConta}</dd>
              </>
            )}
            {cliente.bancoTipoConta && (
              <>
                <dt>Tipo</dt>
                <dd>{TIPO_CONTA[cliente.bancoTipoConta] ?? cliente.bancoTipoConta}</dd>
              </>
            )}
          </dl>
        </section>
      )}
    </div>
  );
}
