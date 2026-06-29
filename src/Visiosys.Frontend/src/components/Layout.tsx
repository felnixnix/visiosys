import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ReactNode } from 'react';

export function Layout({ children }: { children: ReactNode }) {
  const { sair } = useAuth();
  const navigate = useNavigate();

  function handleSair() {
    sair();
    navigate('/login');
  }

  return (
    <div className="layout">
      <header className="header">
        <Link to="/" className="logo">Visiosys</Link>
        <nav>
          <Link to="/">Precatórios</Link>
          <button onClick={handleSair} className="btn-link">Sair</button>
        </nav>
      </header>
      <main className="main">{children}</main>
    </div>
  );
}
