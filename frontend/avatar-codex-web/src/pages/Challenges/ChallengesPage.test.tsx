import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getCharacter } from '../../services/contentService'
import {
  discoverCharacter,
  getChallengeHistory,
  getDailyChallenges,
} from '../../services/userService'
import { challenge, characterDetails, mockSession, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

describe('ChallengesPage', () => {
  it('exige login', async () => {
    const { router } = renderRoute('/challenges')

    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(router.state.location.search).toBe('?redirect=%2Fchallenges')
  })

  it('mostra os desafios de hoje com progresso, concluídos e horário do reset', async () => {
    mockSession()
    vi.mocked(getDailyChallenges).mockResolvedValue({
      date: '2026-10-27',
      resetsAt: '2026-10-28T00:00:00Z',
      challenges: [
        challenge({ progress: 2 }),
        challenge({
          id: 2,
          title: 'Acerte 80% de um quiz',
          type: 'QuizScoreAtLeast',
          progress: 60,
          target: 80,
        }),
        challenge({
          id: 3,
          title: 'Explore uma nação',
          type: 'DiscoverNations',
          progress: 1,
          target: 1,
          isCompleted: true,
          completedAt: '2026-10-27T15:00:00Z',
        }),
      ],
    })
    renderRoute('/challenges')

    expect(await screen.findByText('1 de 3 concluídos')).toBeInTheDocument()
    expect(screen.getByText(/Novos desafios às/)).toBeInTheDocument()
    expect(
      screen.getByRole('progressbar', { name: 'Progresso de Descubra 3 personagens' }),
    ).toHaveAttribute('aria-valuenow', '2')
    expect(screen.getByText('60% de 80%')).toBeInTheDocument()

    const done = screen.getByRole('heading', { level: 3, name: 'Explore uma nação' }).closest('li')!
    expect(within(done).getByText('Concluído')).toBeInTheDocument()
    expect(within(done).queryByRole('progressbar')).not.toBeInTheDocument()
  })

  it('lista o histórico de concluídos e mostra o estado vazio', async () => {
    mockSession()
    vi.mocked(getChallengeHistory).mockResolvedValueOnce(
      page([
        {
          id: 7,
          title: 'Complete um quiz',
          description: '-',
          experienceReward: 30,
          date: '2026-10-26',
          completedAt: '2026-10-26T18:00:00Z',
        },
      ]),
    )
    renderRoute('/challenges')

    const history = await screen.findByRole('region', { name: 'Concluídos' })
    expect(await within(history).findByText('Complete um quiz')).toBeInTheDocument()
    expect(within(history).getByText('+30 XP')).toBeInTheDocument()
  })

  it('histórico vazio e erro nos desafios de hoje', async () => {
    mockSession()
    vi.mocked(getDailyChallenges).mockRejectedValueOnce(new Error('falhou'))
    const events = userEvent.setup()
    renderRoute('/challenges')

    expect(
      await screen.findByText('Os desafios que você concluir aparecem aqui.'),
    ).toBeInTheDocument()
    expect(
      await screen.findByText('Não foi possível carregar os desafios de hoje'),
    ).toBeInTheDocument()

    await events.click(screen.getByRole('button', { name: 'Tentar novamente' }))
    expect(await screen.findByText('0 de 1 concluídos')).toBeInTheDocument()
  })

  it('a navbar tem atalho para os desafios', async () => {
    mockSession()
    renderRoute('/challenges')

    expect(await screen.findByRole('link', { name: 'Desafios diários' })).toHaveAttribute(
      'aria-current',
      'page',
    )
  })
})

describe('desafio concluído (RF-CHAL-02)', () => {
  it('o reward do evento mostra um toast por desafio concluído', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    vi.mocked(discoverCharacter).mockResolvedValue({
      isNew: true,
      reward: {
        xpGained: 40,
        totalXp: 50,
        level: 1,
        leveledUp: false,
        currentLevelXp: 0,
        nextLevelXp: 100,
        levelProgress: 50,
        achievementsUnlocked: [],
        challengesCompleted: [{ id: 3, title: 'Descubra 3 personagens', experienceReward: 30 }],
      },
    })
    renderRoute('/characters/13')

    expect(
      await screen.findByText('Desafio concluído — Descubra 3 personagens! +30 XP'),
    ).toBeInTheDocument()
  })
})
