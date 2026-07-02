import { api } from './client';
import type { ClienteDto, PaginaDto } from '../types';

export interface CriarClientePayload {
  nome: string;
  documento: string;
  email: string;
  telefone?: string;
}

export interface FiltroClientes {
  nome?: string;
  documento?: string;
  tipo?: 'PF' | 'PJ';
  letra?: string;
}

export const clientesApi = {
  listar: (filtro: FiltroClientes = {}, pagina = 1, tamanho = 20) => {
    const params = new URLSearchParams({ pagina: String(pagina), tamanho: String(tamanho) });
    if (filtro.nome) params.set('nome', filtro.nome);
    if (filtro.documento) params.set('documento', filtro.documento);
    if (filtro.tipo) params.set('tipo', filtro.tipo);
    if (filtro.letra) params.set('letra', filtro.letra);
    return api.get<PaginaDto<ClienteDto>>(`/clientes?${params.toString()}`);
  },

  obterPorId: (id: string) =>
    api.get<ClienteDto>(`/clientes/${id}`),

  criar: (payload: CriarClientePayload) =>
    api.post<ClienteDto>('/clientes', payload),
};
