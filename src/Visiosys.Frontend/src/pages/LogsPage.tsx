import { useState, useEffect, useCallback } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts';
import { logsApi } from '../api/logs';
import type { LogEntryDto, LogStatsDto, PaginaDto } from '../types';

const NIVEL_COR: Record<string, string> = {
  Information: '#3b82f6',
  Warning: '#f59e0b',
  Error: '#ef4444',
  Fatal: '#7c3aed',
  Debug: '#6b7280',
  Verbose: '#9ca3af',
};

const NIVEL_CLASS: Record<string, string> = {
  Information: 'badge-blue',
  Warning: 'badge-yellow',
  Error: 'badge-red',
  Fatal: 'badge-purple',
  Debug: 'badge-gray',
  Verbose: 'badge-gray',
};

const NIVEIS_FILTRO = ['', 'Information', 'Warning', 'Error', 'Fatal'];

function formatTs(ts: string) {
  return new Date(ts).toLocaleString('pt-BR', {
    day: '2-digit', month: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  });
}

function StatCard({ titulo, valor, unidade, destaque }: {
  titulo: string; valor: number; unidade?: string; destaque?: boolean;
}) {
  return (
    <div className={`stat-card${destaque ? ' stat-card--destaque' : ''}`}>
      <span className="stat-card__titulo">{titulo}</span>
      <span className="stat-card__valor">
        {valor.toLocaleString('pt-BR')}
        {unidade && <small> {unidade}</small>}
      </span>
    </div>
  );
}

export function LogsPage() {
  const [stats, setStats] = useState<LogStatsDto | null>(null);
  const [logs, setLogs] = useState<PaginaDto<LogEntryDto> | null>(null);
  const [nivel, setNivel] = useState('');
  const [pagina, setPagina] = useState(1);
  const [atualizadoEm, setAtualizadoEm] = useState<Date | null>(null);
  const [expandido, setExpandido] = useState<number | null>(null);

  const carregar = useCallback(async () => {
    try {
      const [s, l] = await Promise.all([
        logsApi.stats(),
        logsApi.listar(nivel || undefined, pagina),
      ]);
      setStats(s);
      setLogs(l);
      setAtualizadoEm(new Date());
    } catch { /* ignora erro silenciosamente no polling */ }
  }, [nivel, pagina]);

  useEffect(() => {
    carregar();
    const id = setInterval(carregar, 15_000);
    return () => clearInterval(id);
  }, [carregar]);

  // Reinicia na página 1 ao mudar filtro
  useEffect(() => { setPagina(1); }, [nivel]);

  const totalPaginas = logs ? Math.ceil(logs.total / logs.tamanho) : 1;

  return (
    <div className="logs-page" data-tour="logs-page">
      <div className="page-header">
        <h1>Logs do Sistema</h1>
        {atualizadoEm && (
          <span className="logs-updated">
            Atualizado às {atualizadoEm.toLocaleTimeString('pt-BR')} · atualiza a cada 15s
          </span>
        )}
      </div>

      {/* Cards de resumo */}
      <div className="stats-grid" data-tour="logs-cards">
        <StatCard titulo="Eventos (24h)" valor={stats?.totalRequests24h ?? 0} />
        <StatCard titulo="Erros (24h)" valor={stats?.erros24h ?? 0} destaque={(stats?.erros24h ?? 0) > 0} />
        <StatCard titulo="Avisos (24h)" valor={stats?.avisos24h ?? 0} />
        <StatCard titulo="Tempo médio" valor={stats?.mediaElapsedMs ?? 0} unidade="ms" />
      </div>

      {/* Gráficos */}
      <div className="charts-grid">
        <div className="chart-card" data-tour="logs-grafico-barras">
          <h3>Eventos por hora (últimas 24h)</h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={stats?.porHora ?? []} margin={{ top: 4, right: 8, left: -16, bottom: 0 }}>
              <XAxis dataKey="hora" tickFormatter={h => `${h}h`} tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} allowDecimals={false} />
              <Tooltip formatter={(v) => [v, 'eventos']} labelFormatter={h => `${h}h`} />
              <Bar dataKey="total" fill="#1d4ed8" radius={[3, 3, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="chart-card" data-tour="logs-grafico-pizza">
          <h3>Distribuição por nível</h3>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie
                data={stats?.porNivel ?? []}
                dataKey="total"
                nameKey="nivel"
                cx="50%" cy="50%"
                outerRadius={70}
                label={({ nivel, percent }) => `${nivel} ${(percent * 100).toFixed(0)}%`}
                labelLine={false}
              >
                {(stats?.porNivel ?? []).map((entry) => (
                  <Cell key={entry.nivel} fill={NIVEL_COR[entry.nivel] ?? '#9ca3af'} />
                ))}
              </Pie>
              <Legend formatter={(v) => v} />
              <Tooltip formatter={(v, n) => [v, n]} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Tabela */}
      <div className="card" data-tour="logs-tabela">
        <div className="logs-toolbar">
          <div className="btn-group">
            {NIVEIS_FILTRO.map(n => (
              <button
                key={n || 'todos'}
                className={`btn-filtro${nivel === n ? ' ativo' : ''}`}
                onClick={() => setNivel(n)}
              >
                {n || 'Todos'}
              </button>
            ))}
          </div>
          <span className="logs-count">{logs?.total ?? 0} registros</span>
        </div>

        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Nível</th>
                <th>Método</th>
                <th>Caminho</th>
                <th>Status</th>
                <th>ms</th>
                <th>Mensagem</th>
              </tr>
            </thead>
            <tbody>
              {logs?.items.length === 0 && (
                <tr><td colSpan={7} className="empty">Nenhum registro encontrado.</td></tr>
              )}
              {logs?.items.map((log, i) => (
                <>
                  <tr
                    key={i}
                    className={log.excecao ? 'row-clicavel' : ''}
                    onClick={() => log.excecao && setExpandido(expandido === i ? null : i)}
                  >
                    <td className="log-ts">{formatTs(log.timestamp)}</td>
                    <td>
                      <span className={`badge ${NIVEL_CLASS[log.level] ?? 'badge-gray'}`}>
                        {log.level}
                      </span>
                    </td>
                    <td>{log.metodo ?? '—'}</td>
                    <td className="log-path" title={log.caminho ?? ''}>{log.caminho ?? '—'}</td>
                    <td>{log.statusCode ?? '—'}</td>
                    <td>{log.elapsedMs != null ? log.elapsedMs : '—'}</td>
                    <td className="log-msg" title={log.mensagem}>{log.mensagem}</td>
                  </tr>
                  {expandido === i && log.excecao && (
                    <tr key={`exc-${i}`} className="row-excecao">
                      <td colSpan={7}>
                        <pre className="excecao-pre">{log.excecao}</pre>
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </div>

        {totalPaginas > 1 && (
          <div className="pagination">
            <button onClick={() => setPagina(p => Math.max(1, p - 1))} disabled={pagina === 1}>
              ‹ Anterior
            </button>
            <span>Página {pagina} de {totalPaginas}</span>
            <button onClick={() => setPagina(p => Math.min(totalPaginas, p + 1))} disabled={pagina === totalPaginas}>
              Próxima ›
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
