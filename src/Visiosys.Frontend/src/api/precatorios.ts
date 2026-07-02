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

export interface FiltroPrecatorios {
  numero?: string;
  tribunal?: string;
  esfera?: string;
  status?: string;
  natureza?: string;
}

export const precatoriosApi = {
  listar: (filtro: FiltroPrecatorios = {}, pagina = 1, tamanho = 20) => {
    const params = new URLSearchParams({ pagina: String(pagina), tamanho: String(tamanho) });
    if (filtro.numero) params.set('numero', filtro.numero);
    if (filtro.tribunal) params.set('tribunal', filtro.tribunal);
    if (filtro.esfera) params.set('esfera', filtro.esfera);
    if (filtro.status) params.set('status', filtro.status);
    if (filtro.natureza) params.set('natureza', filtro.natureza);
    return api.get<PaginaDto<PrecatorioDto>>(`/precatorios?${params.toString()}`);
  },

  obterPorId: (id: string) =>
    api.get<PrecatorioDto>(`/precatorios/${id}`),

  criar: (payload: CriarPrecatorioPayload) =>
    api.post<PrecatorioDto>('/precatorios', payload),
};
