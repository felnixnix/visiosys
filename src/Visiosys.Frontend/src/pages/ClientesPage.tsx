import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { clientesApi } from '../api/clientes';
import type { PaginaDto, ClienteDto } from '../types';

function fmtDoc(doc: string) {
  if (doc.length === 11)
    return doc.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  return doc.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
}

export function ClientesPage() {
  const [dados, setDados] = useState<PaginaDto<ClienteDto> | null>(null);
  const [pagina, setPagina] = useState(1);
  const [erro, setErro] = useState('');
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    let cancelado = false;
    setErro('');
    setDados(null);
    clientesApi.listar(pagina)
      .then(res => { if (!cancelado) setDados(res); })
      .catch(() => { if (!cancelado) setErro('Erro ao carregar clientes.'); });
    return () => { cancelado = true; };
  }, [pagina, tentativa]);

  const totalPaginas = dados ? Math.ceil(dados.total / dados.tamanho) : 0;

  return (
    <div>
      <div className="page-header">
        <h2>Clientes</h2>
        <Link to="/clientes/novo" className="btn-primary">+ Novo</Link>
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
        <p className="vazio">Nenhum cliente cadastrado.</p>
      ) : (
        <>
          <table className="tabela">
            <thead>
              <tr>
                <th>Nome</th>
                <th>CPF / CNPJ</th>
                <th>Email</th>
                <th>Telefone</th>
              </tr>
            </thead>
            <tbody>
              {dados.items.map(c => (
                <tr key={c.id}>
                  <td><Link to={`/clientes/${c.id}`}>{c.nome}</Link></td>
                  <td style={{ fontFamily: 'monospace', fontSize: '.85rem' }}>{fmtDoc(c.documento)}</td>
                  <td>{c.email}</td>
                  <td>{c.telefone ?? '—'}</td>
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
