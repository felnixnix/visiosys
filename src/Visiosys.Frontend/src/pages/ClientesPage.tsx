import { useState, useEffect, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { clientesApi, type FiltroClientes } from '../api/clientes';
import type { PaginaDto, ClienteDto } from '../types';

const LETRAS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.split('');

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

  // Filtro aplicado (o que vai para a API)
  const [filtro, setFiltro] = useState<FiltroClientes>({});
  // Rascunho dos campos de texto/tipo (só aplicam ao clicar em Filtrar)
  const [nomeInput, setNomeInput] = useState('');
  const [docInput, setDocInput] = useState('');
  const [tipoInput, setTipoInput] = useState<'' | 'PF' | 'PJ'>('');

  useEffect(() => {
    let cancelado = false;
    setErro('');
    setDados(null);
    clientesApi.listar(filtro, pagina)
      .then(res => { if (!cancelado) setDados(res); })
      .catch(() => { if (!cancelado) setErro('Erro ao carregar clientes.'); });
    return () => { cancelado = true; };
  }, [filtro, pagina, tentativa]);

  // Ao mudar o filtro, volta para a primeira página
  useEffect(() => { setPagina(1); }, [filtro]);

  function aplicarCampos(e: FormEvent) {
    e.preventDefault();
    setFiltro(f => ({
      ...f,
      nome: nomeInput.trim() || undefined,
      documento: docInput.trim() || undefined,
      tipo: tipoInput || undefined,
    }));
  }

  function selecionarLetra(letra: string) {
    setFiltro(f => ({ ...f, letra: letra || undefined }));
  }

  function limpar() {
    setNomeInput('');
    setDocInput('');
    setTipoInput('');
    setFiltro({});
  }

  const totalPaginas = dados ? Math.ceil(dados.total / dados.tamanho) : 0;
  const temFiltro = !!(filtro.nome || filtro.documento || filtro.tipo || filtro.letra);

  return (
    <div>
      <div className="page-header">
        <h2>Clientes</h2>
        <Link to="/clientes/novo" className="btn-primary">+ Novo</Link>
      </div>

      <div className="filtros">
        <div className="filtro-letras">
          <button className={!filtro.letra ? 'ativo' : ''} onClick={() => selecionarLetra('')}>Todos</button>
          {LETRAS.map(l => (
            <button
              key={l}
              className={filtro.letra === l ? 'ativo' : ''}
              onClick={() => selecionarLetra(l)}
            >
              {l}
            </button>
          ))}
        </div>

        <form className="filtro-campos" onSubmit={aplicarCampos}>
          <div className="campo">
            <label htmlFor="f-nome">Nome</label>
            <input id="f-nome" value={nomeInput} onChange={e => setNomeInput(e.target.value)} placeholder="Ex: Ana Souza" />
          </div>
          <div className="campo">
            <label htmlFor="f-doc">CPF / CNPJ</label>
            <input id="f-doc" value={docInput} onChange={e => setDocInput(e.target.value)} placeholder="Ex: 123.456.789-00" />
          </div>
          <div className="campo campo--estreito">
            <label htmlFor="f-tipo">Tipo</label>
            <select id="f-tipo" value={tipoInput} onChange={e => setTipoInput(e.target.value as '' | 'PF' | 'PJ')}>
              <option value="">Todos</option>
              <option value="PF">Pessoa Física</option>
              <option value="PJ">Pessoa Jurídica</option>
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
        <p className="vazio">{temFiltro ? 'Nenhum cliente encontrado para o filtro.' : 'Nenhum cliente cadastrado.'}</p>
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
