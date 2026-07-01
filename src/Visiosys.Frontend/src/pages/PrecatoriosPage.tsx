import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { precatoriosApi } from '../api/precatorios';
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

function formatBRL(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

export function PrecatoriosPage() {
  const [dados, setDados] = useState<PaginaDto<PrecatorioDto> | null>(null);
  const [pagina, setPagina] = useState(1);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;
    setErro('');
    precatoriosApi.listar(pagina)
      .then(res => { if (!cancelado) setDados(res); })
      .catch(() => { if (!cancelado) setErro('Erro ao carregar precatórios.'); });
    return () => { cancelado = true; };
  }, [pagina]);

  const totalPaginas = dados ? Math.ceil(dados.total / dados.tamanho) : 0;

  return (
    <div>
      <div className="page-header">
        <h2>Precatórios</h2>
        <Link to="/precatorios/novo" className="btn-primary" data-tour="btn-novo">+ Novo</Link>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {!dados ? (
        <p className="carregando">Carregando…</p>
      ) : dados.items.length === 0 ? (
        <p className="vazio">Nenhum precatório cadastrado.</p>
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

          <div className="paginacao">
            <button
              onClick={() => setPagina(p => p - 1)}
              disabled={pagina <= 1}
              className="btn-secondary"
            >
              ← Anterior
            </button>
            <span>{pagina} / {totalPaginas} ({dados.total} registros)</span>
            <button
              onClick={() => setPagina(p => p + 1)}
              disabled={pagina >= totalPaginas}
              className="btn-secondary"
            >
              Próxima →
            </button>
          </div>
        </>
      )}
    </div>
  );
}
