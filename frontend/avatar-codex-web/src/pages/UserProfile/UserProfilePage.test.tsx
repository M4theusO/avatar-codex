import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { followUser, unfollowUser } from '../../services/followService'
import { getPublicProfile } from '../../services/profileService'
import { apiError, fire, mockSession } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { PublicProfile } from '../../types/profile'

vi.mock('../../services/profileService')
vi.mock('../../services/followService')

function profile(overrides: Partial<PublicProfile> = {}): PublicProfile {
  return {
    username: 'zuko',
    avatarUrl: null,
    element: fire,
    level: 4,
    experiencePoints: 520,
    currentLevelXp: 450,
    nextLevelXp: 700,
    levelProgress: 28,
    createdAt: '2026-10-26T12:00:00Z',
    isCurrentUser: false,
    isProfilePrivate: false,
    followersCount: 3,
    followingCount: 1,
    isFollowing: false,
    statistics: {
      discoveredCharacters: 12,
      totalCharacters: 37,
      discoveredNations: 3,
      totalNations: 5,
      discoveredAbilities: 4,
      totalAbilities: 20,
      favorites: 2,
      unlockedAchievements: 2,
      totalAchievements: 10,
      completedQuizzes: 1,
      totalQuizzes: 4,
      quizAttempts: 3,
      accuracyPercent: 70,
    },
    achievements: [
      {
        id: 2,
        name: 'Explorador',
        description: 'Descubra 10 personagens',
        icon: 'compass',
        experienceReward: 50,
        unlockedAt: '2026-11-01T12:00:00Z',
      },
      {
        id: 1,
        name: 'Primeiro passo',
        description: 'Crie sua conta',
        icon: 'footprints',
        experienceReward: 10,
        unlockedAt: '2026-10-26T12:00:00Z',
      },
    ],
    ...overrides,
  }
}

describe('UserProfilePage', () => {
  it('exige login', async () => {
    const { router } = renderRoute('/users/zuko')
    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(getPublicProfile).not.toHaveBeenCalled()
  })

  it('mostra nível, elemento, estatísticas e conquistas, sem atalhos de edição', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockResolvedValue(profile())
    renderRoute('/users/zuko')

    expect(await screen.findByRole('heading', { level: 1, name: 'zuko' })).toBeInTheDocument()
    expect(getPublicProfile).toHaveBeenCalledWith('zuko')
    expect(screen.getByText('Nível 4')).toBeInTheDocument()
    expect(screen.getByText('Fogo')).toBeInTheDocument()
    expect(screen.getByText('70%')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Ver conquistas' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /Editar perfil/ })).not.toBeInTheDocument()

    const achievements = screen.getByRole('heading', { name: 'Conquistas (2)' }).closest('section')!
    const names = within(achievements as HTMLElement).getAllByRole('heading', { level: 3 })
    expect(names.map((heading) => heading.textContent)).toEqual(['Explorador', 'Primeiro passo'])
    expect(within(achievements as HTMLElement).getAllByText(/Desbloqueada em/)).toHaveLength(2)
  })

  it('seguir muda o botão e o número de seguidores na hora', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockResolvedValue(profile())
    vi.mocked(followUser).mockResolvedValue()
    renderRoute('/users/zuko')
    const user = userEvent.setup()

    const follow = await screen.findByRole('button', { name: 'Seguir' })
    expect(follow).toHaveAttribute('aria-pressed', 'false')
    expect(screen.getByText('Seguidores').nextElementSibling).toHaveTextContent('3')
    // Depois do clique, a API devolve o perfil já seguido.
    vi.mocked(getPublicProfile).mockResolvedValue(profile({ isFollowing: true, followersCount: 4 }))
    await user.click(follow)

    expect(followUser).toHaveBeenCalledWith('zuko')
    expect(await screen.findByRole('button', { name: 'Seguindo' })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
    expect(screen.getByText('Seguidores').nextElementSibling).toHaveTextContent('4')
  })

  it('deixar de seguir e recusa da API: o botão volta e a mensagem aparece', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockResolvedValue(profile({ isFollowing: true, followersCount: 4 }))
    vi.mocked(unfollowUser).mockRejectedValue(apiError(500, 'Erro inesperado.'))
    renderRoute('/users/zuko')
    const user = userEvent.setup()

    await user.click(await screen.findByRole('button', { name: 'Seguindo' }))

    expect(unfollowUser).toHaveBeenCalledWith('zuko')
    expect(await screen.findByText(/Ocorreu um erro inesperado/)).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Seguindo' })).toBeInTheDocument()
    expect(screen.getByText('Seguidores').nextElementSibling).toHaveTextContent('4')
  })

  it('o próprio perfil privado avisa e leva à edição', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockResolvedValue(
      profile({ username: 'toph', isCurrentUser: true, isProfilePrivate: true, achievements: [] }),
    )
    renderRoute('/users/toph')

    expect(await screen.findByText(/só você vê esta página/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Editar perfil/ })).toHaveAttribute('href', '/profile')
    expect(screen.queryByRole('button', { name: 'Seguir' })).not.toBeInTheDocument()
    expect(screen.getByText('Nenhuma conquista desbloqueada ainda')).toBeInTheDocument()
  })

  it('inexistente ou privado: mesma mensagem, sem nova tentativa', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockRejectedValue(apiError(404, 'Perfil não encontrado.'))
    renderRoute('/users/ninguem')

    expect(await screen.findByText('Perfil não encontrado')).toBeInTheDocument()
    expect(screen.getByText(/deixou o perfil privado/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Ver ranking' })).toBeInTheDocument()
  })

  it('erro de rede: permite tentar de novo', async () => {
    mockSession()
    vi.mocked(getPublicProfile).mockRejectedValue(new Error('offline'))
    renderRoute('/users/zuko')

    expect(await screen.findByText('Não foi possível carregar o perfil')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})
