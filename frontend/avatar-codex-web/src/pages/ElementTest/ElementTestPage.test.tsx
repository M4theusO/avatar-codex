import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getElementTest, submitElementTest } from '../../services/elementTestService'
import { apiError, fire, mockSession, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { ElementTest, ElementTestResult } from '../../types/elementTest'

vi.mock('../../services/elementTestService')

const water = { id: 1, name: 'Água', color: '#3186B5', icon: 'droplets' }

const test: ElementTest = {
  experienceReward: 30,
  questions: [
    {
      id: 1,
      text: 'Diante de um problema difícil, você costuma…',
      options: [
        { id: 11, text: 'Se adaptar.' },
        { id: 13, text: 'Partir para a ação.' },
      ],
    },
    {
      id: 2,
      text: 'O que mais te motiva?',
      options: [
        { id: 21, text: 'Cuidar das pessoas.' },
        { id: 23, text: 'Superar limites.' },
      ],
    },
  ],
}

function result(overrides: Partial<ElementTestResult> = {}): ElementTestResult {
  return {
    element: { ...fire, description: 'O elemento do poder.', bendingSubtypes: [] },
    scores: [
      { element: water, points: 0 },
      { element: fire, points: 2 },
    ],
    experienceEarned: 30,
    reward: {
      xpGained: 30,
      totalXp: 40,
      level: 1,
      leveledUp: false,
      currentLevelXp: 40,
      nextLevelXp: 100,
      levelProgress: 40,
      achievementsUnlocked: [],
      challengesCompleted: [],
    },
    ...overrides,
  }
}

async function answerAll() {
  const u = userEvent.setup()
  await u.click(await screen.findByRole('button', { name: /Começar/ }))
  await u.click(screen.getByRole('radio', { name: /Partir para a ação/ }))
  await u.click(screen.getByRole('button', { name: /Próxima/ }))
  await u.click(screen.getByRole('radio', { name: /Superar limites/ }))
  await u.click(screen.getByRole('button', { name: /Ver meu elemento/ }))
  return u
}

describe('ElementTestPage', () => {
  it('exige login', async () => {
    const { router } = renderRoute('/element-test')

    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
  })

  it('primeira vez: responde, vê o elemento calculado pelo backend e ganha XP', async () => {
    mockSession()
    vi.mocked(getElementTest).mockResolvedValue(test)
    vi.mocked(submitElementTest).mockResolvedValue(result())
    renderRoute('/element-test')

    expect(
      await screen.findByText('Ganhe +30 XP ao concluir o teste pela primeira vez.'),
    ).toBeInTheDocument()
    await answerAll()

    expect(submitElementTest).toHaveBeenCalledWith([
      { questionId: 1, optionId: 13 },
      { questionId: 2, optionId: 23 },
    ])
    expect(await screen.findByRole('heading', { level: 2, name: 'Fogo' })).toHaveFocus()
    expect(screen.getByText('O elemento do poder.')).toBeInTheDocument()
    expect(screen.getByRole('progressbar', { name: 'Respostas de Fogo' })).toHaveAttribute(
      'aria-valuenow',
      '2',
    )
    expect(await screen.findByText('Elemento descoberto — Fogo! +30 XP')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Ver meu perfil' })).toHaveAttribute('href', '/profile')
  })

  it('refazer: mostra o elemento atual e não anuncia XP', async () => {
    mockSession(user({ element: water }))
    vi.mocked(getElementTest).mockResolvedValue(test)
    vi.mocked(submitElementTest).mockResolvedValue(result({ experienceEarned: 0, reward: null }))
    renderRoute('/element-test')

    const intro = await screen.findByRole('region', { name: 'Sobre o teste' })
    expect(within(intro).getByText('Água')).toBeInTheDocument()
    expect(within(intro).getByText(/Refazer o teste substitui o resultado/)).toBeInTheDocument()
    const u = await answerAll()

    expect(await screen.findByRole('heading', { level: 2, name: 'Fogo' })).toBeInTheDocument()
    expect(screen.queryByText(/Elemento descoberto/)).not.toBeInTheDocument()

    // "Refazer o teste" volta para a primeira pergunta, sem respostas.
    await u.click(screen.getByRole('button', { name: /Refazer o teste/ }))
    expect(screen.getByText('Pergunta 1 de 2')).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: /Partir para a ação/ })).not.toBeChecked()
  })

  it('erro ao enviar: mantém as respostas e mostra a mensagem', async () => {
    mockSession()
    vi.mocked(getElementTest).mockResolvedValue(test)
    vi.mocked(submitElementTest).mockRejectedValue(apiError(400, 'Falta responder 1 pergunta.'))
    renderRoute('/element-test')

    await answerAll()

    expect(await screen.findByRole('alert')).toHaveTextContent('Falta responder 1 pergunta.')
    expect(screen.getByRole('radio', { name: /Superar limites/ })).toBeChecked()
  })

  it('erro ao carregar as perguntas: tentar de novo', async () => {
    mockSession()
    vi.mocked(getElementTest).mockRejectedValueOnce(new Error('falhou')).mockResolvedValue(test)
    const u = userEvent.setup()
    renderRoute('/element-test')

    await u.click(await screen.findByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByRole('button', { name: /Começar/ })).toBeInTheDocument()
  })
})

describe('ProfilePage · elemento', () => {
  it('sem elemento convida para o teste', async () => {
    mockSession()
    renderRoute('/profile')

    expect(await screen.findByRole('link', { name: 'Descobrir meu elemento' })).toHaveAttribute(
      'href',
      '/element-test',
    )
  })

  it('com elemento oferece refazer o teste', async () => {
    mockSession(user({ element: fire }))
    renderRoute('/profile')

    expect(await screen.findByRole('link', { name: 'Refazer o teste' })).toHaveAttribute(
      'href',
      '/element-test',
    )
  })
})
