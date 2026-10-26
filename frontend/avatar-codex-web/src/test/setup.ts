import '@testing-library/jest-dom/vitest'
import { cleanup, configure } from '@testing-library/react'
import { afterEach, beforeEach, vi } from 'vitest'
import { restoreSession } from '../services/authService'
import * as userService from '../services/userService'

// As páginas são lazy: com a suíte inteira em paralelo, o primeiro import pode passar de 1s.
configure({ asyncUtilTimeout: 3000 })

// jsdom não implementa rolagem.
window.scrollTo = vi.fn() as typeof window.scrollTo

// Nenhum teste fala com a API de verdade: por padrão o visitante não tem sessão e o
// usuário não tem favoritos nem descobertas. Use `mockSession` (test/fixtures.ts) para logar.
vi.mock('../services/authService')
vi.mock('../services/userService')

const emptyPage = { items: [], page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }

beforeEach(() => {
  vi.mocked(restoreSession).mockRejectedValue(new Error('Sem sessão'))
  vi.mocked(userService.discoverCharacter).mockResolvedValue({ isNew: false, reward: null })
  vi.mocked(userService.discoverNation).mockResolvedValue({ isNew: false, reward: null })
  vi.mocked(userService.addFavorite).mockResolvedValue()
  vi.mocked(userService.removeFavorite).mockResolvedValue()
  vi.mocked(userService.getFavorites).mockResolvedValue(emptyPage)
  vi.mocked(userService.getDiscoveries).mockResolvedValue(emptyPage)
  vi.mocked(userService.getStatistics).mockResolvedValue({
    discoveredCharacters: 0,
    totalCharacters: 37,
    discoveredNations: 0,
    totalNations: 5,
    favorites: 0,
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})
