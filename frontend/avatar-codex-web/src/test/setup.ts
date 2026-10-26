import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, beforeEach, vi } from 'vitest'
import { restoreSession } from '../services/authService'

// jsdom não implementa rolagem.
window.scrollTo = vi.fn() as typeof window.scrollTo

// Nenhum teste fala com a API de verdade: por padrão o visitante não tem sessão.
// Use `mockSession` (test/fixtures.ts) para simular um usuário logado.
vi.mock('../services/authService')

beforeEach(() => {
  vi.mocked(restoreSession).mockRejectedValue(new Error('Sem sessão'))
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})
