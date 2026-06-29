import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ReactNode } from 'react';

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { logado } = useAuth();
  return logado ? <>{children}</> : <Navigate to="/login" replace />;
}
