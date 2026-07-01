import { describe, it, expect } from 'vitest';
import { passosTour } from './steps';

describe('passosTour', () => {
  it('todo passo declara a rota do seu alvo em data.route', () => {
    for (const passo of passosTour) {
      expect(passo.data?.route).toBeTruthy();
    }
  });

  it('apresenta a página de logs com ao menos 3 passos', () => {
    const passosLogs = passosTour.filter((p) => p.data.route === '/logs');
    expect(passosLogs.length).toBeGreaterThanOrEqual(3);
  });

  it('o primeiro passo de /logs aguarda o alvo aparecer após a navegação', () => {
    const primeiroLogs = passosTour.find((p) => p.data.route === '/logs');
    expect(primeiroLogs?.targetWaitTimeout).toBeGreaterThan(0);
  });

  it('começa com um passo de boas-vindas centralizado na home', () => {
    expect(passosTour[0].placement).toBe('center');
    expect(passosTour[0].data.route).toBe('/');
  });
});
