import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { precatoriosApi } from '../api/precatorios';
import { ApiError } from '../api/client';

export function NovoPrecatorioPage() {
  const navigate = useNavigate();
  const [numero, setNumero] = useState('');
  const [tribunal, setTribunal] = useState('');
  const [valorFace, setValorFace] = useState('');
  const [esfera, setEsfera] = useState('2');
  const [natureza, setNatureza] = useState('1');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      await precatoriosApi.criar({
        numero,
        tribunalOrigem: tribunal,
        valorFace: parseFloat(valorFace.replace(',', '.')),
        esfera: parseInt(esfera),
        natureza: parseInt(natureza),
      });
      navigate('/');
    } catch (err) {
      setErro(
        err instanceof ApiError
          ? `Erro ${err.status}: ${err.message}`
          : 'Erro inesperado.'
      );
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="form-container">
      <h2>Novo Precatório</h2>
      <form onSubmit={handleSubmit} className="form">
        <div className="field">
          <label htmlFor="numero">Número do Precatório</label>
          <input id="numero" value={numero} onChange={e => setNumero(e.target.value)} required />
        </div>
        <div className="field">
          <label htmlFor="tribunal">Tribunal de Origem</label>
          <input id="tribunal" value={tribunal} onChange={e => setTribunal(e.target.value)} required />
        </div>
        <div className="field">
          <label htmlFor="valorFace">Valor de Face (R$)</label>
          <input
            id="valorFace"
            type="number"
            step="0.01"
            min="0.01"
            value={valorFace}
            onChange={e => setValorFace(e.target.value)}
            required
          />
        </div>
        <div className="field">
          <label htmlFor="esfera">Esfera</label>
          <select id="esfera" value={esfera} onChange={e => setEsfera(e.target.value)}>
            <option value="1">Municipal</option>
            <option value="2">Estadual</option>
            <option value="3">Federal</option>
          </select>
        </div>
        <div className="field">
          <label htmlFor="natureza">Natureza</label>
          <select id="natureza" value={natureza} onChange={e => setNatureza(e.target.value)}>
            <option value="1">Comum</option>
            <option value="2">Alimentar</option>
          </select>
        </div>
        {erro && <p className="erro">{erro}</p>}
        <div className="actions">
          <button type="button" className="btn-secondary" onClick={() => navigate('/')}>
            Cancelar
          </button>
          <button type="submit" className="btn-primary" disabled={carregando}>
            {carregando ? 'Salvando…' : 'Salvar'}
          </button>
        </div>
      </form>
    </div>
  );
}
