import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { PrecatoriosPage } from './pages/PrecatoriosPage';
import { NovoPrecatorioPage } from './pages/NovoPrecatorioPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Layout>
                  <PrecatoriosPage />
                </Layout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/precatorios/novo"
            element={
              <ProtectedRoute>
                <Layout>
                  <NovoPrecatorioPage />
                </Layout>
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
