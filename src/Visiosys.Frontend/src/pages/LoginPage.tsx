import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/auth';
import { ApiError } from '../api/client';
import { useAuth } from '../contexts/AuthContext';

function IconeOlho({ aberto }: { aberto: boolean }) {
  if (aberto) {
    return (
      <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
        fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>
        <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>
        <line x1="1" y1="1" x2="23" y2="23"/>
      </svg>
    );
  }
  return (
    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
      fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
      <circle cx="12" cy="12" r="3"/>
    </svg>
  );
}

export function LoginPage() {
  const [loginVal, setLoginVal] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
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
            <div className="input-senha">
              <input
                id="senha"
                type={mostrarSenha ? 'text' : 'password'}
                value={senha}
                onChange={e => setSenha(e.target.value)}
                required
              />
              <button
                type="button"
                className="toggle-senha"
                onClick={() => setMostrarSenha(v => !v)}
                aria-label={mostrarSenha ? 'Ocultar senha' : 'Revelar senha'}
              >
                <IconeOlho aberto={mostrarSenha} />
              </button>
            </div>
          </div>
          {erro && <p className="erro">{erro}</p>}
          <button type="submit" className="btn-primary" disabled={carregando}>
            {carregando ? 'Entrando…' : 'Entrar'}
          </button>
        </form>
        <div className="demo-hint">
          <strong>Acesso demo</strong>
          <span>Login: <code>user</code> &nbsp;&middot;&nbsp; Senha: <code>user</code></span>
        </div>
      </div>
    </div>
  );
}
