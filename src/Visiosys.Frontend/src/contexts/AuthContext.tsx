import { createContext, useContext, useState, type ReactNode } from 'react';
import { clearToken } from '../api/client';

interface AuthContextValue {
  logado: boolean;
  entrar: () => void;
  sair: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [logado, setLogado] = useState(() => !!sessionStorage.getItem('visiosys_token'));

  function entrar() {
    setLogado(true);
  }

  function sair() {
    clearToken();
    setLogado(false);
  }

  return (
    <AuthContext.Provider value={{ logado, entrar, sair }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
