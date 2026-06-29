import { getToken } from './client';
import type { DocumentoDto, TipoDocumento } from '../types';
import { ApiError } from './client';

export const documentosApi = {
  listarPorPrecatorio: (precatorioId: string) =>
    fetch(`/api/documentos?precatorioId=${precatorioId}`, {
      headers: { Authorization: `Bearer ${getToken()}` },
    }).then(async (res) => {
      if (!res.ok) throw new ApiError(res.status, res.statusText);
      return res.json() as Promise<DocumentoDto[]>;
    }),

  upload: async (
    arquivo: File,
    tipo: TipoDocumento,
    precatorioId?: string,
  ): Promise<DocumentoDto> => {
    const form = new FormData();
    form.append('arquivo', arquivo);
    form.append('tipo', tipo);
    if (precatorioId) form.append('precatorioId', precatorioId);

    const res = await fetch('/api/documentos', {
      method: 'POST',
      headers: { Authorization: `Bearer ${getToken()}` },
      body: form,
    });

    if (!res.ok) {
      const body = await res.json().catch(() => ({ erro: res.statusText }));
      throw new ApiError(res.status, body.erro ?? res.statusText);
    }

    return res.json();
  },
};
