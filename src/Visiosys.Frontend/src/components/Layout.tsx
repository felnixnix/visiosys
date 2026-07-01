import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Joyride, STATUS, ACTIONS, EVENTS, type EventData, type ButtonType } from 'react-joyride';
import { useAuth } from '../contexts/AuthContext';
import { passosTour } from '../tour/steps';
import type { ReactNode } from 'react';

const CHAVE_TOUR = 'visiosys_tour_feito';
const HEALTH_URL = `${import.meta.env.BASE_URL}health`;

type StatusSaude = 'verificando' | 'online' | 'offline';

const opcoesTour = {
  primaryColor: '#1d4ed8',
  zIndex: 1000,
  overlayColor: 'rgba(0,0,0,0.45)',
  buttons: ['back', 'primary', 'skip'] as ButtonType[],
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
  const [passoTour, setPassoTour] = useState(0);
  const [saude, setSaude] = useState<StatusSaude>('verificando');

  useEffect(() => {
    async function verificar() {
      try {
        const res = await fetch(HEALTH_URL);
        setSaude(res.ok ? 'online' : 'offline');
      } catch {
        setSaude('offline');
      }
    }
    verificar();
    const id = setInterval(verificar, 30_000);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    if (location.pathname === '/' && !localStorage.getItem(CHAVE_TOUR)) {
      const t = setTimeout(() => {
        setPassoTour(0);
        setRodandoTour(true);
      }, 800);
      return () => clearTimeout(t);
    }
  }, [location.pathname]);

  function encerrarTour() {
    localStorage.setItem(CHAVE_TOUR, '1');
    setRodandoTour(false);
    setPassoTour(0);
  }

  function handleTourEvento(data: EventData) {
    const { type, action, index, status } = data;

    // Fim do tour: concluído, pulado ou fechado (X / ESC / overlay).
    if (
      status === STATUS.FINISHED ||
      status === STATUS.SKIPPED ||
      action === ACTIONS.CLOSE ||
      action === ACTIONS.SKIP
    ) {
      encerrarTour();
      return;
    }

    // Transição de passo. O tour está em modo controlado, então avançamos o
    // índice manualmente e, quando o próximo passo pertence a outra rota,
    // navegamos antes — o Joyride aguarda o alvo aparecer (targetWaitTimeout).
    if (type === EVENTS.STEP_AFTER) {
      const proximo = index + (action === ACTIONS.PREV ? -1 : 1);
      if (proximo < 0 || proximo >= passosTour.length) {
        encerrarTour();
        return;
      }
      const rota = passosTour[proximo]?.data?.route;
      if (rota && rota !== location.pathname) {
        navigate(rota);
      }
      setPassoTour(proximo);
    }
  }

  function handleIniciarTour() {
    setPassoTour(0);
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
        stepIndex={passoTour}
        onEvent={handleTourEvento}
        continuous
        scrollToFirstStep
        locale={localeTour}
        options={opcoesTour}
      />
      <header className="header">
        <Link to="/" className="logo">Visiosys</Link>
        <nav>
          <Link to="/" data-tour="nav-precatorios">Precatórios</Link>
          <Link to="/clientes" data-tour="nav-clientes">Clientes</Link>
          <Link to="/logs" data-tour="nav-logs">Logs</Link>
          <Link to="/ajuda" data-tour="nav-ajuda">Ajuda</Link>
          <span
            className="status-saude"
            data-status={saude}
            title={
              saude === 'online' ? 'Sistema operacional — PostgreSQL e MongoDB respondendo'
              : saude === 'offline' ? 'Serviço indisponível'
              : 'Verificando...'
            }
          >
            {saude === 'online' ? 'online' : saude === 'offline' ? 'offline' : '...'}
          </span>
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
