import type { AuthResponse, LoginData, RegisterData } from '../types/auth'
import { api, refreshSession, setAccessToken } from './api'

function startSession(data: AuthResponse) {
  setAccessToken(data.accessToken)
  return data.user
}

export async function login(data: LoginData) {
  const response = await api.post<AuthResponse>('/api/auth/login', data)
  return startSession(response.data)
}

export async function register(data: RegisterData) {
  const response = await api.post<AuthResponse>('/api/auth/register', data)
  return startSession(response.data)
}

/** Recupera a sessão ao abrir/recarregar a página, usando o cookie de refresh. */
export async function restoreSession() {
  return (await refreshSession()).user
}

export async function logout() {
  try {
    await api.post('/api/auth/logout')
  } finally {
    // Mesmo se a API falhar, a sessão local termina.
    setAccessToken(null)
  }
}
