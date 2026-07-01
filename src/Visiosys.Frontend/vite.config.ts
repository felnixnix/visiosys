import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ command }) => ({
  plugins: [react()],
  // Em produção a API responde sob /visiosys (ver ADR-023); em dev local
  // (`npm run dev`) o app continua servido na raiz, sem prefixo.
  base: command === 'build' ? '/visiosys/' : '/',
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/health': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: '../Visiosys.Api/wwwroot',
    emptyOutDir: true,
  },
}));
