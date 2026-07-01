import { test, expect } from '@playwright/test';

// Smoke E2E do fluxo principal: login com a conta demo e navegação pelas telas
// centrais, validando que renderizam e que a API respondeu (as listas populam).
test.describe('Fluxo principal', () => {
  test.beforeEach(async ({ page }) => {
    // Suprime o tour automático para não sobrepor os elementos verificados.
    await page.addInitScript(() => {
      try { localStorage.setItem('visiosys_tour_feito', '1'); } catch { /* noop */ }
    });
  });

  test('login e navegação por precatórios, logs, ajuda e detalhe', async ({ page }) => {
    // Login com a conta de demonstração
    await page.goto('login');
    await expect(page.locator('#login')).toBeVisible();
    await page.fill('#login', 'user');
    await page.fill('#senha', 'user');
    await page.click('button[type="submit"]');

    // Lista de precatórios (layout autenticado + dados da API)
    await expect(page.locator('[data-tour="nav-logs"]')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Precatórios' })).toBeVisible();
    await expect(page.locator('[data-tour="tabela"] tbody tr').first()).toBeVisible();

    // Dashboard de logs
    await page.click('[data-tour="nav-logs"]');
    await expect(page.getByRole('heading', { name: 'Logs do Sistema' })).toBeVisible();
    await expect(page.locator('[data-tour="logs-cards"]')).toBeVisible();
    await expect(page.locator('[data-tour="logs-tabela"]')).toBeVisible();

    // Ajuda/API
    await page.click('[data-tour="nav-ajuda"]');
    await expect(page.getByRole('heading', { name: 'Ajuda/API' })).toBeVisible();
    await expect(page.getByText('Acesso à API')).toBeVisible();

    // Detalhe de um precatório (navega pela lista)
    await page.click('[data-tour="nav-precatorios"]');
    await page.click('[data-tour="tabela"] tbody tr:first-child a');
    await expect(page.getByText('Tribunal de Origem')).toBeVisible();
    await expect(page.getByText('Valor de Face')).toBeVisible();
  });
});
