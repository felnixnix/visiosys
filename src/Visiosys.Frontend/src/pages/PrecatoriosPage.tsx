import { useState, useEffect, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { precatoriosApi, type FiltroPrecatorios } from '../api/precatorios';
import type { PaginaDto, PrecatorioDto } from '../types';

const STATUS_LABEL: Record<string, string> = {
  EmAnalise: 'Em Análise',
  AguardandoDocumentacao: 'Ag. Documentação',
  EmNegociacao: 'Em Negociação',
  AguardandoPagamento: 'Ag. Pagamento',
  Liquidado: 'Liquidado',
  Cancelado: 'Cancelado',
};

const STATUS_CLASS: Record<string, string> = {
  EmAnalise: 'badge-blue',
  AguardandoDocumentacao: 'badge-yellow',
  EmNegociacao: 'badge-orange',
  AguardandoPagamento: 'badge-purple',
  Liquidado: 'badge-green',
  Cancelado: 'badge-red',
};

const ESFERAS = ['Municipal', 'Estadual', 'Federal'];
const NATUREZAS = ['Comum', 'Alimentar'];

function formatBRL(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

export function PrecatoriosPage() {
  const [dados, setDados] = useState<PaginaDto<PrecatorioDto> | null>(null);
  const [pagina, setPagina] = useState(1);
  const [erro, setErro] = useState('');
  const [tentativa, setTentativa] = useState(0);

  // Filtro aplicado (o que vai para a API)
  const [filtro, setFiltro] = useState<FiltroPrecatorios>({});
  // Rascunho dos campos (só aplicam ao clicar em Filtrar)
  const [numeroInput, setNumeroInput] = useState('');
  const [tribunalInput, setTribunalInput] = useState('');
  const [esferaInput, setEsferaInput] = useState('');
  const [statusInput, setStatusInput] = useState('');
  const [naturezaInput, setNaturezaInput] = useState('');

  useEffect(() => {
    let cancelado = false;
    setErro('');
    setDados(null);
    precatoriosApi.listar(filtro, pagina)
      .then(res => { if (!cancelado) setDados(res); })
      .catch(() => { if (!cancelado) setErro('Erro ao carregar precatórios.'); });
    return () => { cancelado = true; };
  }, [filtro, pagina, tentativa]);

  // Ao mudar o filtro, volta para a primeira página
  useEffect(() => { setPagina(1); }, [filtro]);

  function aplicar(e: FormEvent) {
    e.preventDefault();
    setFiltro({
      numero: numeroInput.trim() || undefined,
      tribunal: tribunalInput.trim() || undefined,
      esfera: esferaInput || undefined,
      status: statusInput || undefined,
      natureza: naturezaInput || undefined,
    });
  }

  function limpar() {
    setNumeroInput('');
    setTribunalInput('');
    setEsferaInput('');
    setStatusInput('');
    setNaturezaInput('');
    setFiltro({});
  }

  const totalPaginas = dados ? Math.ceil(dados.total / dados.tamanho) : 0;
  const temFiltro = !!(filtro.numero || filtro.tribunal || filtro.esfera || filtro.status || filtro.natureza);

  return (
    <div>
      <div className="page-header">
        <h2>Precatórios</h2>
        <Link to="/precatorios/novo" className="btn-primary" data-tour="btn-novo">+ Novo</Link>
      </div>

      <div className="filtros">
        <form className="filtro-campos" onSubmit={aplicar}>
          <div className="campo">
            <label htmlFor="f-numero">Número do processo</label>
            <input id="f-numero" value={numeroInput} onChange={e => setNumeroInput(e.target.value)} placeholder="Ex: 0001234-56" />
          </div>
          <div className="campo">
            <label htmlFor="f-tribunal">Tribunal</label>
            <input id="f-tribunal" value={tribunalInput} onChange={e => setTribunalInput(e.target.value)} placeholder="Ex: TJSP" />
          </div>
          <div className="campo campo--estreito">
            <label htmlFor="f-esfera">Esfera</label>
            <select id="f-esfera" value={esferaInput} onChange={e => setEsferaInput(e.target.value)}>
              <option value="">Todas</option>
              {ESFERAS.map(x => <option key={x} value={x}>{x}</option>)}
            </select>
          </div>
          <div className="campo campo--estreito">
            <label htmlFor="f-status">Status</label>
            <select id="f-status" value={statusInput} onChange={e => setStatusInput(e.target.value)}>
              <option value="">Todos</option>
              {Object.entries(STATUS_LABEL).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
          </div>
          <div className="campo campo--estreito">
            <label htmlFor="f-natureza">Natureza</label>
            <select id="f-natureza" value={naturezaInput} onChange={e => setNaturezaInput(e.target.value)}>
              <option value="">Todas</option>
              {NATUREZAS.map(x => <option key={x} value={x}>{x}</option>)}
            </select>
          </div>
          <button type="submit" className="btn-primary">Filtrar</button>
          {temFiltro && (
            <button type="button" className="btn-secondary" onClick={limpar}>Limpar</button>
          )}
        </form>
      </div>

      {erro ? (
        <div className="estado-erro">
          <p className="erro">{erro}</p>
          <button className="btn-secondary" onClick={() => setTentativa(t => t + 1)}>
            Tentar novamente
          </button>
        </div>
      ) : !dados ? (
        <p className="carregando">Carregando…</p>
      ) : dados.items.length === 0 ? (
        <p className="vazio">{temFiltro ? 'Nenhum precatório encontrado para o filtro.' : 'Nenhum precatório cadastrado.'}</p>
      ) : (
        <>
          <table className="tabela" data-tour="tabela">
            <thead>
              <tr>
                <th>Número</th>
                <th>Tribunal</th>
                <th>Valor Face</th>
                <th>Esfera</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {dados.items.map(p => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/precatorios/${p.id}`}>{p.numero}</Link>
                  </td>
                  <td>{p.tribunalOrigem}</td>
                  <td>{formatBRL(p.valorFace)}</td>
                  <td>{p.esfera}</td>
                  <td>
                    <span className={`badge ${STATUS_CLASS[p.status] ?? ''}`} data-tour="badge-status">
                      {STATUS_LABEL[p.status] ?? p.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {totalPaginas > 1 && (
            <div className="paginacao">
              <button onClick={() => setPagina(p => p - 1)} disabled={pagina <= 1} className="btn-secondary">
                ← Anterior
              </button>
              <span>{pagina} / {totalPaginas} ({dados.total} registros)</span>
              <button onClick={() => setPagina(p => p + 1)} disabled={pagina >= totalPaginas} className="btn-secondary">
                Próxima →
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
