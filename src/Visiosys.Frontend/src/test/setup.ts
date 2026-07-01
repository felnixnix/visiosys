// Registra os matchers do jest-dom (toBeInTheDocument, toHaveAttribute, etc.)
// na instância de expect do Vitest.
import '@testing-library/jest-dom/vitest';
import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

// Com globals: false, o cleanup automático do Testing Library não se registra
// sozinho — desmontamos o DOM após cada teste para não vazar entre casos.
afterEach(() => {
  cleanup();
});
