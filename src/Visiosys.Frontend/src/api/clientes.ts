import { api } from './client';
import type { ClienteDto, PaginaDto } from '../types';

export interface CriarClientePayload {
  nome: string;
  documento: string;
  email: string;
  telefone?: string;
}

export const clientesApi = {
  listar: (pagina = 1, tamanho = 20) =>
    api.get<PaginaDto<ClienteDto>>(`/clientes?pagina=${pagina}&tamanho=${tamanho}`),

  obterPorId: (id: string) =>
    api.get<ClienteDto>(`/clientes/${id}`),

  criar: (payload: CriarClientePayload) =>
    api.post<ClienteDto>('/clientes', payload),
};
