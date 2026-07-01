import { lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';

// Páginas protegidas carregadas sob demanda (code splitting): cada rota vira um
// chunk separado, tirando dependências pesadas (ex.: recharts na LogsPage) do
// bundle inicial. O LoginPage fica eager por ser a porta de entrada.
const PrecatoriosPage = lazy(() => import('./pages/PrecatoriosPage').then(m => ({ default: m.PrecatoriosPage })));
const NovoPrecatorioPage = lazy(() => import('./pages/NovoPrecatorioPage').then(m => ({ default: m.NovoPrecatorioPage })));
const PrecatorioDetalhePage = lazy(() => import('./pages/PrecatorioDetalhePage').then(m => ({ default: m.PrecatorioDetalhePage })));
const ClientesPage = lazy(() => import('./pages/ClientesPage').then(m => ({ default: m.ClientesPage })));
const NovoClientePage = lazy(() => import('./pages/NovoClientePage').then(m => ({ default: m.NovoClientePage })));
const ClienteDetalhePage = lazy(() => import('./pages/ClienteDetalhePage').then(m => ({ default: m.ClienteDetalhePage })));
const AjudaPage = lazy(() => import('./pages/AjudaPage').then(m => ({ default: m.AjudaPage })));
const LogsPage = lazy(() => import('./pages/LogsPage').then(m => ({ default: m.LogsPage })));

function ProtectedLayout({ children }: { children: React.ReactNode }) {
  return (
    <ProtectedRoute>
      <Layout>{children}</Layout>
    </ProtectedRoute>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter basename={import.meta.env.BASE_URL}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<ProtectedLayout><PrecatoriosPage /></ProtectedLayout>} />
          <Route path="/precatorios/novo" element={<ProtectedLayout><NovoPrecatorioPage /></ProtectedLayout>} />
          <Route path="/precatorios/:id" element={<ProtectedLayout><PrecatorioDetalhePage /></ProtectedLayout>} />
          <Route path="/clientes" element={<ProtectedLayout><ClientesPage /></ProtectedLayout>} />
          <Route path="/clientes/novo" element={<ProtectedLayout><NovoClientePage /></ProtectedLayout>} />
          <Route path="/clientes/:id" element={<ProtectedLayout><ClienteDetalhePage /></ProtectedLayout>} />
          <Route path="/logs" element={<ProtectedLayout><LogsPage /></ProtectedLayout>} />
          <Route path="/ajuda" element={<ProtectedLayout><AjudaPage /></ProtectedLayout>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
