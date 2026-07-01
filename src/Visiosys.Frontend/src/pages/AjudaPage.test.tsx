import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AjudaPage } from './AjudaPage';

describe('AjudaPage', () => {
  it('exibe o título Ajuda/API', () => {
    render(<AjudaPage />);
    expect(screen.getByRole('heading', { name: 'Ajuda/API' })).toBeInTheDocument();
  });

  it('mostra o box de acesso à API com as credenciais demo', () => {
    render(<AjudaPage />);
    expect(screen.getByText('Acesso à API')).toBeInTheDocument();
    // Login e senha ("user") aparecem como dois <code>.
    expect(screen.getAllByText('user')).toHaveLength(2);
  });

  it('linka para a documentação Swagger', () => {
    render(<AjudaPage />);
    const link = screen.getByRole('link', { name: '/swagger' });
    expect(link).toHaveAttribute('href', expect.stringContaining('swagger'));
  });
});
