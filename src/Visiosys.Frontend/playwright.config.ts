import { defineConfig, devices } from '@playwright/test';

// E2E sob demanda (não faz parte do CI nem do hook pre-push).
// Por padrão roda contra o ambiente ao vivo; use E2E_BASE_URL para apontar
// para um ambiente local (ex.: http://localhost:5173/).
// Requer o Google Chrome instalado (channel: 'chrome') — não baixa browsers.
export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  fullyParallel: true,
  reporter: [['list']],
  use: {
    baseURL: process.env.E2E_BASE_URL || 'https://felipedearaujo.dev/visiosys/',
    headless: true,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    },
  ],
});
