import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { precatoriosApi } from '../api/precatorios';
import { andamentosApi } from '../api/andamentos';
import { pagamentosApi } from '../api/pagamentos';
import { documentosApi } from '../api/documentos';
import { clientesApi } from '../api/clientes';
import type {
  PrecatorioDto,
  AndamentoDto,
  PagamentoDto,
  DocumentoDto,
  TipoAndamento,
  TipoDocumento,
  ClienteDto,
} from '../types';

const STATUS_LABELS: Record<string, string> = {
  EmAnalise: 'Em Análise',
  AguardandoDocumentacao: 'Ag. Documentação',
  EmNegociacao: 'Em Negociação',
  AguardandoPagamento: 'Ag. Pagamento',
  Liquidado: 'Liquidado',
  Cancelado: 'Cancelado',
};

const STATUS_BADGE: Record<string, string> = {
  EmAnalise: 'badge-blue',
  AguardandoDocumentacao: 'badge-yellow',
  EmNegociacao: 'badge-orange',
  AguardandoPagamento: 'badge-purple',
  Liquidado: 'badge-green',
  Cancelado: 'badge-red',
};

const TIPO_ANDAMENTO_LABELS: Record<string, string> = {
  AtualizacaoStatus: 'Atualização de Status',
  DocumentoRecebido: 'Documento Recebido',
  ContatoRealizado: 'Contato Realizado',
  ObservacaoInterna: 'Observação Interna',
  PropostaEnviada: 'Proposta Enviada',
  PropostaRecebida: 'Proposta Recebida',
  PagamentoRegistrado: 'Pagamento Registrado',
};

const TIPO_DOCUMENTO_LABELS: Record<string, string> = {
  Procuracao: 'Procuração',
  Certidao: 'Certidão',
  Contrato: 'Contrato',
  Peticao: 'Petição',
  Outro: 'Outro',
};

function fmt(valor: number) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function fmtData(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
}

function fmtBytes(bytes: number) {
  return bytes < 1024 * 1024
    ? `${(bytes / 1024).toFixed(0)} KB`
    : `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

type Aba = 'info' | 'andamentos' | 'pagamentos' | 'documentos';

export function PrecatorioDetalhePage() {
  const { id } = useParams<{ id: string }>();
  const [precatorio, setPrecatorio] = useState<PrecatorioDto | null>(null);
  const [cliente, setCliente] = useState<ClienteDto | null>(null);
  const [andamentos, setAndamentos] = useState<AndamentoDto[]>([]);
  const [pagamentos, setPagamentos] = useState<PagamentoDto[]>([]);
  const [documentos, setDocumentos] = useState<DocumentoDto[]>([]);
  const [aba, setAba] = useState<Aba>('info');
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  // Formulário andamento
  const [novoAndDesc, setNovoAndDesc] = useState('');
  const [novoAndTipo, setNovoAndTipo] = useState<TipoAndamento>('ObservacaoInterna');
  const [salvandoAnd, setSalvandoAnd] = useState(false);

  // Formulário pagamento
  const [novoPagValor, setNovoPagValor] = useState('');
  const [novoPagData, setNovoPagData] = useState('');
  const [salvandoPag, setSalvandoPag] = useState(false);

  // Formulário documento
  const [novoDocArquivo, setNovoDocArquivo] = useState<File | null>(null);
  const [novoDocTipo, setNovoDocTipo] = useState<TipoDocumento>('Outro');
  const [salvandoDoc, setSalvandoDoc] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      precatoriosApi.obterPorId(id),
      andamentosApi.listar(id),
      pagamentosApi.listar(id),
      documentosApi.listarPorPrecatorio(id),
    ])
      .then(([p, a, pg, d]) => {
        setPrecatorio(p);
        setAndamentos(a);
        setPagamentos(pg);
        setDocumentos(d);
        if (p.clienteId) {
          clientesApi.obterPorId(p.clienteId).then(setCliente).catch(() => null);
        }
      })
      .catch(() => setErro('Erro ao carregar precatório.'))
      .finally(() => setCarregando(false));
  }, [id]);

  async function registrarAndamento(e: React.FormEvent) {
    e.preventDefault();
    if (!id || !novoAndDesc.trim()) return;
    setSalvandoAnd(true);
    try {
      const dto = await andamentosApi.registrar(id, novoAndDesc.trim(), novoAndTipo);
      setAndamentos((prev) => [dto, ...prev]);
      setNovoAndDesc('');
    } catch {
      setErro('Erro ao registrar andamento.');
    } finally {
      setSalvandoAnd(false);
    }
  }

  async function registrarPagamento(e: React.FormEvent) {
    e.preventDefault();
    if (!id || !novoPagValor) return;
    setSalvandoPag(true);
    try {
      const dto = await pagamentosApi.registrar(
        id,
        parseFloat(novoPagValor.replace(',', '.')),
        novoPagData || undefined,
      );
      setPagamentos((prev) => [dto, ...prev]);
      setNovoPagValor('');
      setNovoPagData('');
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : 'Erro ao registrar pagamento.');
    } finally {
      setSalvandoPag(false);
    }
  }

  async function enviarDocumento(e: React.FormEvent) {
    e.preventDefault();
    if (!id || !novoDocArquivo) return;
    setSalvandoDoc(true);
    try {
      const dto = await documentosApi.upload(novoDocArquivo, novoDocTipo, id);
      setDocumentos((prev) => [dto, ...prev]);
      setNovoDocArquivo(null);
      (e.target as HTMLFormElement).reset();
    } catch {
      setErro('Erro ao enviar documento. Apenas PDFs são aceitos.');
    } finally {
      setSalvandoDoc(false);
    }
  }

  if (carregando) return <p className="carregando">Carregando...</p>;
  if (erro && !precatorio) return <p className="erro">{erro}</p>;
  if (!precatorio) return <p className="vazio">Precatório não encontrado.</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <Link to="/" className="btn-link" style={{ color: 'var(--gray-600)', fontSize: '.8rem' }}>
            ← Precatórios
          </Link>
          <h2 style={{ margin: '.25rem 0 0' }}>{precatorio.numero}</h2>
        </div>
        <span className={`badge ${STATUS_BADGE[precatorio.status]}`}>
          {STATUS_LABELS[precatorio.status]}
        </span>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {/* Abas */}
      <div className="abas">
        {(['info', 'andamentos', 'pagamentos', 'documentos'] as Aba[]).map((a) => (
          <button
            key={a}
            className={`aba-btn ${aba === a ? 'aba-ativa' : ''}`}
            onClick={() => setAba(a)}
          >
            {a === 'info' && 'Informações'}
            {a === 'andamentos' && `Andamentos (${andamentos.length})`}
            {a === 'pagamentos' && `Pagamentos (${pagamentos.length})`}
            {a === 'documentos' && `Documentos (${documentos.length})`}
          </button>
        ))}
      </div>

      {/* Aba: Informações */}
      {aba === 'info' && (
        <div className="form" style={{ marginTop: '1rem' }}>
          <dl className="info-grid">
            <dt>Tribunal de Origem</dt><dd>{precatorio.tribunalOrigem}</dd>
            <dt>Esfera</dt><dd>{precatorio.esfera}</dd>
            <dt>Natureza</dt><dd>{precatorio.natureza}</dd>
            <dt>Valor de Face</dt><dd>{fmt(precatorio.valorFace)}</dd>
            <dt>Valor Atualizado</dt>
            <dd>{precatorio.valorAtualizado != null ? fmt(precatorio.valorAtualizado) : '—'}</dd>
            <dt>Cliente</dt>
            <dd>
              {cliente
                ? <Link to={`/clientes/${cliente.id}`}>{cliente.nome}</Link>
                : precatorio.clienteId
                  ? <span style={{ color: 'var(--gray-600)', fontSize: '.85rem' }}>Carregando…</span>
                  : '—'}
            </dd>
            <dt>Cadastrado em</dt><dd>{fmtData(precatorio.criadoEm)}</dd>
          </dl>
        </div>
      )}

      {/* Aba: Andamentos */}
      {aba === 'andamentos' && (
        <div style={{ marginTop: '1rem' }}>
          <form onSubmit={registrarAndamento} className="form" style={{ marginBottom: '1rem' }}>
            <h3 style={{ margin: '0 0 1rem' }}>Novo Andamento</h3>
            <div className="field">
              <label>Tipo</label>
              <select value={novoAndTipo} onChange={(e) => setNovoAndTipo(e.target.value as TipoAndamento)}>
                {Object.entries(TIPO_ANDAMENTO_LABELS).map(([v, l]) => (
                  <option key={v} value={v}>{l}</option>
                ))}
              </select>
            </div>
            <div className="field">
              <label>Descrição</label>
              <textarea
                value={novoAndDesc}
                onChange={(e) => setNovoAndDesc(e.target.value)}
                rows={3}
                style={{ width: '100%', padding: '.5rem .75rem', border: '1px solid var(--gray-200)', borderRadius: 'var(--radius)', fontSize: '.9rem', resize: 'vertical' }}
                placeholder="Descreva o andamento..."
                required
              />
            </div>
            <div className="actions">
              <button type="submit" className="btn-primary" disabled={salvandoAnd}>
                {salvandoAnd ? 'Salvando...' : 'Registrar'}
              </button>
            </div>
          </form>

          {andamentos.length === 0 ? (
            <p className="vazio">Nenhum andamento registrado.</p>
          ) : (
            <div className="timeline">
              {andamentos.map((a) => (
                <div key={a.id} className="timeline-item">
                  <div className="timeline-header">
                    <span className="badge badge-blue">{TIPO_ANDAMENTO_LABELS[a.tipo]}</span>
                    <span style={{ color: 'var(--gray-600)', fontSize: '.8rem' }}>{fmtData(a.ocorridoEm)}</span>
                  </div>
                  <p style={{ margin: '.5rem 0 0' }}>{a.descricao}</p>
                  <span style={{ color: 'var(--gray-600)', fontSize: '.75rem' }}>por {a.registradoPorLogin}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Aba: Pagamentos */}
      {aba === 'pagamentos' && (
        <div style={{ marginTop: '1rem' }}>
          {precatorio.status !== 'Liquidado' && precatorio.status !== 'Cancelado' && (
            <form onSubmit={registrarPagamento} className="form" style={{ marginBottom: '1rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>Registrar Pagamento</h3>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="field">
                  <label>Valor Pago (R$)</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0.01"
                    value={novoPagValor}
                    onChange={(e) => setNovoPagValor(e.target.value)}
                    placeholder="0,00"
                    required
                  />
                </div>
                <div className="field">
                  <label>Data do Pagamento</label>
                  <input
                    type="date"
                    value={novoPagData}
                    onChange={(e) => setNovoPagData(e.target.value)}
                  />
                </div>
              </div>
              <div className="actions">
                <button type="submit" className="btn-primary" disabled={salvandoPag}>
                  {salvandoPag ? 'Salvando...' : 'Registrar Pagamento'}
                </button>
              </div>
            </form>
          )}

          {pagamentos.length === 0 ? (
            <p className="vazio">Nenhum pagamento registrado.</p>
          ) : (
            <table className="tabela">
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Valor Pago</th>
                  <th>Valor Base</th>
                  <th>Deságio</th>
                  <th>Registrado por</th>
                </tr>
              </thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td>{fmtData(p.pagoEm)}</td>
                    <td>{fmt(p.valorPago)}</td>
                    <td>{fmt(p.valorBase)}</td>
                    <td>{p.percDesagio.toFixed(2)}%</td>
                    <td>{p.registradoPorLogin}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Aba: Documentos */}
      {aba === 'documentos' && (
        <div style={{ marginTop: '1rem' }}>
          <form onSubmit={enviarDocumento} className="form" style={{ marginBottom: '1rem' }}>
            <h3 style={{ margin: '0 0 1rem' }}>Enviar Documento</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
              <div className="field">
                <label>Tipo</label>
                <select value={novoDocTipo} onChange={(e) => setNovoDocTipo(e.target.value as TipoDocumento)}>
                  {Object.entries(TIPO_DOCUMENTO_LABELS).map(([v, l]) => (
                    <option key={v} value={v}>{l}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label>Arquivo PDF</label>
                <input
                  type="file"
                  accept="application/pdf"
                  onChange={(e) => setNovoDocArquivo(e.target.files?.[0] ?? null)}
                  required
                />
              </div>
            </div>
            <div className="actions">
              <button type="submit" className="btn-primary" disabled={salvandoDoc || !novoDocArquivo}>
                {salvandoDoc ? 'Enviando...' : 'Enviar'}
              </button>
            </div>
          </form>

          {documentos.length === 0 ? (
            <p className="vazio">Nenhum documento enviado.</p>
          ) : (
            <table className="tabela">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Tipo</th>
                  <th>Tamanho</th>
                  <th>Enviado por</th>
                  <th>Data</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {documentos.map((d) => (
                  <tr key={d.id}>
                    <td>{d.nomeOriginal}</td>
                    <td>{TIPO_DOCUMENTO_LABELS[d.tipo]}</td>
                    <td>{fmtBytes(d.tamanhoBytes)}</td>
                    <td>{d.enviadoPorLogin}</td>
                    <td>{fmtData(d.criadoEm)}</td>
                    <td>
                      <a href={d.urlDownload} target="_blank" rel="noreferrer" className="btn-link" style={{ color: 'var(--primary)' }}>
                        Download
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
