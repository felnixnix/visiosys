import { api } from './client';
import type { PaginaDto, PrecatorioDto } from '../types';

export function listarPrecatorios(pagina = 1, tamanho = 20) {
  return api.get<PaginaDto<PrecatorioDto>>(
    `/precatorios?pagina=${pagina}&tamanho=${tamanho}`
  );
}

export function obterPrecatorio(id: string) {
  return api.get<PrecatorioDto>(`/precatorios/${id}`);
}

export interface CriarPrecatorioPayload {
  numero: string;
  tribunalOrigem: string;
  valorFace: number;
  esfera: number;
  natureza: number;
}

export function criarPrecatorio(payload: CriarPrecatorioPayload) {
  return api.post<PrecatorioDto>('/precatorios', payload);
}
