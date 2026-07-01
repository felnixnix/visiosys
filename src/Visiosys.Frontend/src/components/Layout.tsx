import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import Joyride, { type CallBackProps, STATUS } from 'react-joyride';
import { useAuth } from '../contexts/AuthContext';
import { passosTour } from '../tour/steps';
import type { ReactNode } from 'react';

const CHAVE_TOUR = 'visiosys_tour_feito';

const estilosTour = {
  options: {
    primaryColor: '#1d4ed8',
    zIndex: 1000,
    arrowColor: '#fff',
    backgroundColor: '#fff',
    overlayColor: 'rgba(0,0,0,0.45)',
    textColor: '#1f2937',
    width: 320,
  },
};

const localeTour = {
  back: 'Voltar',
  close: 'Fechar',
  last: 'Concluir',
  next: 'Próximo',
  skip: 'Pular tour',
};

export function Layout({ children }: { children: ReactNode }) {
  const { sair } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [rodandoTour, setRodandoTour] = useState(false);

  useEffect(() => {
    if (location.pathname === '/' && !localStorage.getItem(CHAVE_TOUR)) {
      const t = setTimeout(() => setRodandoTour(true), 800);
      return () => clearTimeout(t);
    }
  }, [location.pathname]);

  function handleTourCallback(data: CallBackProps) {
    if (data.status === STATUS.FINISHED || data.status === STATUS.SKIPPED) {
      localStorage.setItem(CHAVE_TOUR, '1');
      setRodandoTour(false);
    }
  }

  function handleIniciarTour() {
    if (location.pathname !== '/') {
      navigate('/');
      setTimeout(() => setRodandoTour(true), 400);
    } else {
      setRodandoTour(true);
    }
  }

  function handleSair() {
    sair();
    navigate('/login');
  }

  return (
    <div className="layout">
      <Joyride
        steps={passosTour}
        run={rodandoTour}
        callback={handleTourCallback}
        continuous
        showSkipButton
        scrollToFirstStep
        locale={localeTour}
        styles={estilosTour}
      />
      <header className="header">
        <Link to="/" className="logo">Visiosys</Link>
        <nav>
          <Link to="/" data-tour="nav-precatorios">Precatórios</Link>
          <Link to="/clientes" data-tour="nav-clientes">Clientes</Link>
          <Link to="/ajuda" data-tour="nav-ajuda">Ajuda</Link>
          <button
            className="btn-tour"
            onClick={handleIniciarTour}
            data-tour="btn-tour"
            title="Iniciar tour guiado"
            aria-label="Iniciar tour guiado"
          >
            ?
          </button>
          <button onClick={handleSair} className="btn-link">Sair</button>
        </nav>
      </header>
      <main className="main">{children}</main>
    </div>
  );
}
