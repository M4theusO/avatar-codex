import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getDashboard } from '../../services/userService'
import { achievement, character, dashboard, mockSession, quiz, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

describe('DashboardPage', () => {
  it('exige login e volta para o painel depois de entrar', async () => {
    const { router } = renderRoute('/dashboard')

    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(router.state.location.search).toBe('?redirect=%2Fdashboard')
  })

  it('usuário novo: saudação, progresso zerado e convites para começar', async () => {
    mockSession()
    renderRoute('/dashboard')

    expect(await screen.findByRole('heading', { level: 1, name: 'Olá, toph' })).toBeInTheDocument()
    expect(
      await screen.findByRole('progressbar', { name: 'Personagens descobertos' }),
    ).toHaveAttribute('aria-valuemax', '37')
    const challenges = screen.getByRole('region', { name: 'Desafios de hoje' })
    expect(
      within(challenges).getByRole('heading', { level: 3, name: 'Descubra 3 personagens' }),
    ).toBeInTheDocument()
    expect(within(challenges).getByText('0 de 1 concluídos')).toBeInTheDocument()
    expect(screen.getByText('Nenhuma conquista desbloqueada ainda.')).toBeInTheDocument()
    expect(screen.getByText('Você gabaritou todos os quizzes. Impressionante!')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Conhecer as nações' })).toHaveAttribute(
      'href',
      '/nations',
    )
    expect(
      screen.getByText('Use a estrela nos cards de personagem para guardar seus favoritos aqui.'),
    ).toBeInTheDocument()
  })

  it('mostra quiz em destaque, conquistas, recomendações, descobertas e favoritos', async () => {
    const signed = mockSession()
    vi.mocked(getDashboard).mockResolvedValue(
      dashboard({
        user: signed,
        featuredQuiz: quiz(),
        recentAchievements: [achievement()],
        nextAchievement: achievement({
          id: 2,
          name: 'Curioso',
          isUnlocked: false,
          unlockedAt: null,
          progress: 3,
          target: 5,
        }),
        recommendations: [character({ id: 7, name: 'Toph Beifong' })],
        recentDiscoveries: [
          {
            kind: 'Character',
            id: 13,
            name: 'Zuko',
            image: null,
            element: null,
            discoveredAt: '2026-10-27T10:00:00Z',
          },
        ],
        favorites: [character({ id: 14, name: 'Iroh', isFavorite: true })],
      }),
    )
    renderRoute('/dashboard')

    expect(await screen.findByRole('link', { name: 'Jogar: O Time Avatar' })).toHaveAttribute(
      'href',
      '/quizzes/1',
    )
    expect(screen.getByText('Primeiro passo')).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 3, name: 'Curioso' })).toBeInTheDocument()
    expect(screen.getByRole('progressbar', { name: 'Progresso de Curioso' })).toHaveAttribute(
      'aria-valuenow',
      '3',
    )

    const explore = screen.getByRole('region', { name: 'Continue explorando' })
    expect(within(explore).getByRole('link', { name: 'Toph Beifong' })).toHaveAttribute(
      'href',
      '/characters/7',
    )
    const discoveries = screen.getByRole('region', { name: 'Descobertas recentes' })
    expect(within(discoveries).getByRole('link', { name: /Zuko/ })).toHaveAttribute(
      'href',
      '/characters/13',
    )
    const favorites = screen.getByRole('region', { name: 'Favoritos' })
    expect(within(favorites).getByRole('link', { name: 'Iroh' })).toHaveAttribute(
      'href',
      '/characters/14',
    )
  })

  it('atualiza o XP da sessão com o valor do servidor', async () => {
    mockSession(user({ experiencePoints: 10 }))
    vi.mocked(getDashboard).mockResolvedValue(
      dashboard({
        user: user({
          experiencePoints: 130,
          level: 2,
          currentLevelXp: 100,
          nextLevelXp: 400,
          levelProgress: 10,
        }),
      }),
    )
    renderRoute('/dashboard')

    await waitFor(() =>
      expect(screen.getAllByRole('progressbar', { name: 'Experiência' })[0]).toHaveAttribute(
        'aria-valuenow',
        '130',
      ),
    )
    expect(screen.getByText('Nível 2')).toBeInTheDocument()
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    mockSession()
    vi.mocked(getDashboard).mockRejectedValueOnce(new Error('falhou'))
    const events = userEvent.setup()
    renderRoute('/dashboard')

    expect(await screen.findByText('Não foi possível carregar seu painel')).toBeInTheDocument()
    // A saudação não depende do dashboard.
    expect(screen.getByRole('heading', { level: 1, name: 'Olá, toph' })).toBeInTheDocument()

    await events.click(screen.getByRole('button', { name: 'Tentar novamente' }))
    expect(
      await screen.findByRole('progressbar', { name: 'Personagens descobertos' }),
    ).toBeInTheDocument()
  })

  it('a navbar leva ao painel para quem está logado', async () => {
    mockSession()
    renderRoute('/quizzes')

    expect(await screen.findByRole('link', { name: 'Painel' })).toHaveAttribute(
      'href',
      '/dashboard',
    )
    expect(screen.queryByRole('link', { name: 'Início' })).not.toBeInTheDocument()
  })
})
