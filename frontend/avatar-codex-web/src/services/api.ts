import axios, { type InternalAxiosRequestConfig } from 'axios'
import type { AuthResponse } from '../types/auth'

export const api = axios.create({
  // Em dev aponta para a API local; em produção fica vazio e as chamadas são relativas (/api/...),
  // repassadas pela Vercel para a API (docs/deployment.md).
  baseURL: import.meta.env.VITE_API_URL,
  // Envia o cookie httpOnly de refresh para /api/auth.
  withCredentials: true,
})

// O access token fica só em memória (nunca em localStorage): recarregar a página
// recupera a sessão pelo cookie de refresh (docs/architecture.md §3.4).
let accessToken: string | null = null
let refreshing: Promise<AuthResponse> | null = null
let onSessionExpired: (() => void) | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

export function setSessionExpiredHandler(handler: (() => void) | null) {
  onSessionExpired = handler
}

/**
 * Um refresh por vez: chamadas simultâneas (várias requisições com 401, StrictMode)
 * compartilham a mesma promessa, porque cada refresh token só pode ser usado uma vez.
 */
export function refreshSession(): Promise<AuthResponse> {
  refreshing ??= api
    .post<AuthResponse>('/api/auth/refresh')
    .then(({ data }) => {
      accessToken = data.accessToken
      return data
    })
    .finally(() => {
      refreshing = null
    })
  return refreshing
}

api.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`
  return config
})

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean }

// Access token expirado: renova uma vez e repete a requisição original.
api.interceptors.response.use(undefined, async (error) => {
  const config = error.config as RetriableConfig | undefined
  const isAuthEndpoint = config?.url?.startsWith('/api/auth/')

  if (error.response?.status !== 401 || !config || config._retried || isAuthEndpoint) {
    throw error
  }

  config._retried = true
  try {
    await refreshSession()
  } catch {
    accessToken = null
    onSessionExpired?.()
    throw error
  }
  return api(config)
})
