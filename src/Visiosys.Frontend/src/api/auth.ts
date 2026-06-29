import { api, setToken } from './client';
import type { TokenDto } from '../types';

export async function login(login: string, senha: string): Promise<TokenDto> {
  const dto = await api.post<TokenDto>('/auth/login', { login, senha });
  setToken(dto.token);
  return dto;
}
