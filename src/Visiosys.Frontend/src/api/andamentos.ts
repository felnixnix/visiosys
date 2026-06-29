import { api } from './client';
import type { AndamentoDto, TipoAndamento } from '../types';

export const andamentosApi = {
  listar: (precatorioId: string) =>
    api.get<AndamentoDto[]>(`/precatorios/${precatorioId}/andamentos`),

  registrar: (precatorioId: string, descricao: string, tipo: TipoAndamento) =>
    api.post<AndamentoDto>(`/precatorios/${precatorioId}/andamentos`, { descricao, tipo }),
};
