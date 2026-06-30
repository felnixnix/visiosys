import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/auth';
import { ApiError } from '../api/client';
import { useAuth } from '../contexts/AuthContext';

export function LoginPage() {
  const [loginVal, setLoginVal] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);
  const { entrar } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      await login(loginVal, senha);
      entrar();
      navigate('/');
    } catch (err) {
      if (err instanceof ApiError) {
        setErro(err.status === 401 ? 'Credenciais inválidas.' : `Erro ao conectar à API (${err.status}).`);
      } else {
        setErro('Erro ao conectar à API.');
      }
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Visiosys</h1>
        <p className="subtitle">Sistema de Gestão de Precatórios</p>
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="login">Login</label>
            <input
              id="login"
              type="text"
              value={loginVal}
              onChange={e => setLoginVal(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="field">
            <label htmlFor="senha">Senha</label>
            <input
              id="senha"
              type="password"
              value={senha}
              onChange={e => setSenha(e.target.value)}
              required
            />
          </div>
          {erro && <p className="erro">{erro}</p>}
          <button type="submit" className="btn-primary" disabled={carregando}>
            {carregando ? 'Entrando…' : 'Entrar'}
          </button>
        </form>
      </div>
    </div>
  );
}
