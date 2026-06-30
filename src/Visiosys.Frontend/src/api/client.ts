export const BASE_URL = `${import.meta.env.BASE_URL}api`;

export function getToken(): string | null {
  return sessionStorage.getItem('visiosys_token');
}

export function setToken(token: string): void {
  sessionStorage.setItem('visiosys_token', token);
}

export function clearToken(): void {
  sessionStorage.removeItem('visiosys_token');
}

export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...init.headers,
  };

  const res = await fetch(`${BASE_URL}${path}`, { ...init, headers });

  if (!res.ok) {
    const body = await res.json().catch(() => ({ erro: res.statusText }));
    throw new ApiError(res.status, body.erro ?? res.statusText);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
};
