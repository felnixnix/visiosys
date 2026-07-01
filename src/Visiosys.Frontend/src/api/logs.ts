import { BASE_URL } from './client';
import type { LogEntryDto, LogStatsDto, PaginaDto } from '../types';

const token = () => sessionStorage.getItem('visiosys_token');

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { Authorization: `Bearer ${token()}` },
  });
  if (!res.ok) throw new Error(res.statusText);
  return res.json();
}

export const logsApi = {
  listar: (nivel?: string, pagina = 1, tamanho = 20) => {
    const params = new URLSearchParams({ pagina: String(pagina), tamanho: String(tamanho) });
    if (nivel) params.set('nivel', nivel);
    return get<PaginaDto<LogEntryDto>>(`/logs?${params}`);
  },
  stats: () => get<LogStatsDto>('/logs/stats'),
};
