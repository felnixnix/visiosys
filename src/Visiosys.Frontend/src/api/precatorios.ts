import { api } from './client';
import type { PaginaDto, PrecatorioDto } from '../types';

export interface CriarPrecatorioPayload {
  numero: string;
  tribunalOrigem: string;
  valorFace: number;
  esfera: number;
  natureza: number;
  clienteId?: string;
}

export const precatoriosApi = {
  listar: (pagina = 1, tamanho = 20) =>
    api.get<PaginaDto<PrecatorioDto>>(`/precatorios?pagina=${pagina}&tamanho=${tamanho}`),

  obterPorId: (id: string) =>
    api.get<PrecatorioDto>(`/precatorios/${id}`),

  criar: (payload: CriarPrecatorioPayload) =>
    api.post<PrecatorioDto>('/precatorios', payload),
};
