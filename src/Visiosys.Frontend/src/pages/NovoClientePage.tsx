import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { clientesApi } from '../api/clientes';
import { ApiError } from '../api/client';

export function NovoClientePage() {
  const navigate = useNavigate();
  const [nome, setNome] = useState('');
  const [documento, setDocumento] = useState('');
  const [email, setEmail] = useState('');
  const [telefone, setTelefone] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      const dto = await clientesApi.criar({
        nome,
        documento: documento.replace(/\D/g, ''),
        email,
        telefone: telefone || undefined,
      });
      navigate(`/clientes/${dto.id}`);
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
      <h2>Novo Cliente</h2>
      <form onSubmit={handleSubmit} className="form">
        <div className="field">
          <label>Nome completo</label>
          <input value={nome} onChange={e => setNome(e.target.value)} required />
        </div>
        <div className="field">
          <label>CPF ou CNPJ</label>
          <input
            value={documento}
            onChange={e => setDocumento(e.target.value)}
            placeholder="000.000.000-00 ou 00.000.000/0000-00"
            required
          />
        </div>
        <div className="field">
          <label>Email</label>
          <input type="email" value={email} onChange={e => setEmail(e.target.value)} required />
        </div>
        <div className="field">
          <label>Telefone</label>
          <input
            value={telefone}
            onChange={e => setTelefone(e.target.value)}
            placeholder="(11) 99999-9999"
          />
        </div>

        {erro && <p className="erro">{erro}</p>}

        <div className="actions">
          <button type="button" className="btn-secondary" onClick={() => navigate('/clientes')}>
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
