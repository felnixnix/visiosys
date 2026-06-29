import { api } from './client';
import type { PagamentoDto } from '../types';

export const pagamentosApi = {
  listar: (precatorioId: string) =>
    api.get<PagamentoDto[]>(`/precatorios/${precatorioId}/pagamentos`),

  registrar: (precatorioId: string, valorPago: number, pagoEm?: string) =>
    api.post<PagamentoDto>(`/precatorios/${precatorioId}/pagamentos`, { valorPago, pagoEm }),
};
