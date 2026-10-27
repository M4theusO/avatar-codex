import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getAchievements } from '../../services/userService'
import { mockSession } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { UserAchievement } from '../../types/progress'

function achievement(overrides: Partial<UserAchievement> = {}): UserAchievement {
  return {
    id: 1,
    name: 'Primeiro passo',
    description: 'Crie sua conta.',
    icon: 'footprints',
    experienceReward: 10,
    isUnlocked: true,
    unlockedAt: '2026-10-26T12:00:00Z',
    progress: 1,
    target: 1,
    ...overrides,
  }
}

describe('AchievementsPage', () => {
  it('mostra desbloqueadas com data e bloqueadas com progresso', async () => {
    mockSession()
    vi.mocked(getAchievements).mockResolvedValue([
      achievement(),
      achievement({
        id: 2,
        name: 'Curioso',
        description: 'Descubra 10 personagens.',
        icon: 'eye',
        experienceReward: 50,
        isUnlocked: false,
        unlockedAt: null,
        progress: 3,
        target: 10,
      }),
    ])
    renderRoute('/achievements')

    expect(await screen.findByText('de 2 conquistas desbloqueadas')).toBeInTheDocument()
    expect(screen.getByRole('progressbar', { name: 'Conquistas desbloqueadas' })).toHaveAttribute(
      'aria-valuenow',
      '1',
    )

    const unlocked = screen.getByRole('heading', { name: 'Primeiro passo' }).closest('li')!
    const locked = screen.getByRole('heading', { name: 'Curioso' }).closest('li')!
    expect(within(unlocked).getByText('Desbloqueada em 26/10/2026')).toBeInTheDocument()
    expect(within(unlocked).queryByRole('progressbar')).not.toBeInTheDocument()

    expect(within(locked).getByText(/Bloqueada/)).toBeInTheDocument()
    expect(
      within(locked).getByRole('progressbar', { name: 'Progresso de Curioso' }),
    ).toHaveAttribute('aria-valuenow', '3')
    expect(within(locked).getByText('+50 XP')).toBeInTheDocument()
  })

  it('mostra erro com opção de tentar novamente', async () => {
    mockSession()
    vi.mocked(getAchievements)
      .mockRejectedValueOnce(new Error('Network Error'))
      .mockResolvedValueOnce([achievement()])
    renderRoute('/achievements')

    expect(await screen.findByText('Não foi possível carregar suas conquistas')).toBeInTheDocument()
    await userEvent.setup().click(screen.getByRole('button', { name: /Tentar novamente/ }))

    expect(await screen.findByRole('heading', { name: 'Primeiro passo' })).toBeInTheDocument()
  })

  it('mostra estado vazio quando não há conquistas cadastradas', async () => {
    mockSession()
    renderRoute('/achievements')

    expect(await screen.findByText('Nenhuma conquista disponível')).toBeInTheDocument()
  })

  it('exige login', async () => {
    const { router } = renderRoute('/achievements')
    await vi.waitFor(() => expect(router.state.location.pathname).toBe('/login'))
  })

  it('tem atalho na navbar para quem está logado', async () => {
    mockSession()
    renderRoute('/')

    expect(await screen.findByRole('link', { name: 'Conquistas' })).toHaveAttribute(
      'href',
      '/achievements',
    )
  })
})
