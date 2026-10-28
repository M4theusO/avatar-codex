import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getDiscoveries, getFavorites, getStatistics } from '../../services/userService'
import { character, mockSession } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

describe('FavoritesPage', () => {
  it('mostra estado vazio com convite para explorar', async () => {
    mockSession()
    renderRoute('/favorites')

    expect(await screen.findByText('Você ainda não possui favoritos')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Explorar personagens' })).toHaveAttribute(
      'href',
      '/characters',
    )
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    mockSession()
    vi.mocked(getFavorites).mockRejectedValueOnce(new Error('falhou'))
    const user = userEvent.setup()
    renderRoute('/favorites')

    await user.click(await screen.findByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByText('Você ainda não possui favoritos')).toBeInTheDocument()
  })

  it('lista os favoritos do usuário', async () => {
    mockSession()
    vi.mocked(getFavorites).mockResolvedValue({
      items: [
        character({ isFavorite: true }),
        character({ id: 14, name: 'Iroh', isFavorite: true }),
      ],
      page: 1,
      pageSize: 12,
      totalItems: 2,
      totalPages: 1,
    })
    renderRoute('/favorites')

    expect(await screen.findByRole('link', { name: 'Zuko' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Iroh' })).toBeInTheDocument()
    expect(getFavorites).toHaveBeenCalledWith(1)
  })
})

describe('ProfilePage (exploração)', () => {
  it('mostra estatísticas e descobertas recentes', async () => {
    mockSession()
    vi.mocked(getStatistics).mockResolvedValue({
      discoveredCharacters: 3,
      totalCharacters: 37,
      discoveredNations: 1,
      totalNations: 5,
      favorites: 2,
      unlockedAchievements: 1,
      totalAchievements: 6,
      completedQuizzes: 1,
      totalQuizzes: 4,
      quizAttempts: 3,
      accuracyPercent: 87,
    })
    vi.mocked(getDiscoveries).mockResolvedValue({
      items: [
        {
          kind: 'Nation',
          id: 4,
          name: 'Nação do Fogo',
          image: null,
          element: null,
          discoveredAt: '2026-10-26T15:00:00Z',
        },
        {
          kind: 'Character',
          id: 13,
          name: 'Zuko',
          image: null,
          element: null,
          discoveredAt: '2026-10-26T14:00:00Z',
        },
      ],
      page: 1,
      pageSize: 6,
      totalItems: 2,
      totalPages: 1,
    })
    renderRoute('/profile')

    expect(
      await screen.findByRole('progressbar', { name: 'Personagens descobertos' }),
    ).toHaveAttribute('aria-valuenow', '3')
    expect(screen.getByRole('progressbar', { name: 'Nações descobertas' })).toHaveAttribute(
      'aria-valuemax',
      '5',
    )
    expect(screen.getByRole('progressbar', { name: 'Conquistas desbloqueadas' })).toHaveAttribute(
      'aria-valuenow',
      '1',
    )
    expect(screen.getByRole('link', { name: 'Ver conquistas' })).toHaveAttribute(
      'href',
      '/achievements',
    )
    expect(screen.getByRole('progressbar', { name: 'Quizzes concluídos' })).toHaveAttribute(
      'aria-valuenow',
      '1',
    )
    expect(screen.getByText('87%')).toBeInTheDocument()
    expect(screen.getByText('3 tentativas')).toBeInTheDocument()
    expect(await screen.findByRole('link', { name: /Nação do Fogo/ })).toHaveAttribute(
      'href',
      '/nations/4',
    )
    expect(screen.getByRole('link', { name: /Zuko/ })).toHaveAttribute('href', '/characters/13')
  })

  it('favoritos exige login', async () => {
    const { router } = renderRoute('/favorites')
    await vi.waitFor(() => expect(router.state.location.pathname).toBe('/login'))
  })
})
