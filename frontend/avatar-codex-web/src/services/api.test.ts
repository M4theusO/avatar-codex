import { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { user } from '../test/fixtures'
import { api, refreshSession, setAccessToken, setSessionExpiredHandler } from './api'

// Servidor falso: só aceita o token "fresh"; o refresh entrega esse token (ou falha).
let refreshSucceeds = true
let requests: { url?: string; authorization?: unknown }[] = []

function respond(config: InternalAxiosRequestConfig, status: number, data: unknown) {
  const response = { status, statusText: '', data, headers: {}, config }
  if (status >= 400) {
    throw new AxiosError(`HTTP ${status}`, 'ERR_BAD_REQUEST', config, null, response)
  }
  return response
}

beforeEach(() => {
  refreshSucceeds = true
  requests = []
  setAccessToken(null)
  api.defaults.adapter = async (config) => {
    const authorization = config.headers.Authorization
    requests.push({ url: config.url, authorization })

    if (config.url === '/api/auth/refresh') {
      return refreshSucceeds
        ? respond(config, 200, { accessToken: 'fresh', expiresIn: 900, user: user() })
        : respond(config, 401, { status: 401, message: 'Sessão expirada.' })
    }
    return authorization === 'Bearer fresh'
      ? respond(config, 200, { ok: true })
      : respond(config, 401, { status: 401, message: 'Você precisa estar autenticado.' })
  }
})

afterEach(() => {
  setSessionExpiredHandler(null)
})

describe('api (sessão)', () => {
  it('renova o access token após 401 e repete a requisição', async () => {
    setAccessToken('expired')

    const response = await api.get('/api/users/me')

    expect(response.data).toEqual({ ok: true })
    expect(requests).toEqual([
      { url: '/api/users/me', authorization: 'Bearer expired' },
      { url: '/api/auth/refresh', authorization: 'Bearer expired' },
      { url: '/api/users/me', authorization: 'Bearer fresh' },
    ])
  })

  it('faz um único refresh quando várias chamadas acontecem juntas', async () => {
    const [first, second] = await Promise.all([refreshSession(), refreshSession()])

    expect(first).toBe(second)
    expect(requests.filter((r) => r.url === '/api/auth/refresh')).toHaveLength(1)
  })

  it('encerra a sessão local quando o refresh falha', async () => {
    const onExpired = vi.fn()
    setSessionExpiredHandler(onExpired)
    setAccessToken('expired')
    refreshSucceeds = false

    await expect(api.get('/api/users/me')).rejects.toMatchObject({ response: { status: 401 } })

    expect(onExpired).toHaveBeenCalledOnce()
    expect(requests.filter((r) => r.url === '/api/users/me')).toHaveLength(1)
  })

  it('não tenta refresh quando o próprio endpoint de auth responde 401', async () => {
    await expect(api.post('/api/auth/login', {})).rejects.toBeInstanceOf(AxiosError)

    expect(requests.map((r) => r.url)).toEqual(['/api/auth/login'])
  })
})
