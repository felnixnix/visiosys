import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { PrecatoriosPage } from './pages/PrecatoriosPage';
import { NovoPrecatorioPage } from './pages/NovoPrecatorioPage';
import { PrecatorioDetalhePage } from './pages/PrecatorioDetalhePage';
import { ClientesPage } from './pages/ClientesPage';
import { NovoClientePage } from './pages/NovoClientePage';
import { ClienteDetalhePage } from './pages/ClienteDetalhePage';
import { AjudaPage } from './pages/AjudaPage';

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
          <Route path="/ajuda" element={<ProtectedLayout><AjudaPage /></ProtectedLayout>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
