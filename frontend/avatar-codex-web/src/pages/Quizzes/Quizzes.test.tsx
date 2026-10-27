import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getQuiz, getQuizHistory, getQuizzes, submitQuizAttempt } from '../../services/quizService'
import { apiError, mockSession, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { Quiz, QuizAttemptResult, QuizListItem } from '../../types/quiz'

function listItem(overrides: Partial<QuizListItem> = {}): QuizListItem {
  return {
    id: 1,
    title: 'O Time Avatar',
    description: 'Aang e seus amigos.',
    category: 'Personagens',
    difficulty: 'Easy',
    image: null,
    questionCount: 10,
    bestResult: null,
    ...overrides,
  }
}

const quiz: Quiz = {
  id: 1,
  title: 'O Time Avatar',
  description: 'Aang e seus amigos.',
  category: 'Personagens',
  difficulty: 'Easy',
  image: null,
  questions: [
    {
      id: 10,
      text: 'Qual é o bisão de Aang?',
      imageUrl: null,
      options: [
        { id: 101, text: 'Momo' },
        { id: 102, text: 'Appa' },
      ],
    },
    {
      id: 11,
      text: 'Quem é o tio de Zuko?',
      imageUrl: null,
      options: [
        { id: 111, text: 'Iroh' },
        { id: 112, text: 'Ozai' },
      ],
    },
  ],
}

function attemptResult(overrides: Partial<QuizAttemptResult> = {}): QuizAttemptResult {
  return {
    attemptId: 7,
    score: 1,
    totalQuestions: 2,
    percentage: 50,
    experienceEarned: 20,
    results: [
      {
        questionId: 10,
        selectedOptionId: 101,
        correctOptionId: 102,
        isCorrect: false,
        explanation: 'Appa é o bisão; Momo é o lêmure.',
      },
      {
        questionId: 11,
        selectedOptionId: 111,
        correctOptionId: 111,
        isCorrect: true,
        explanation: null,
      },
    ],
    reward: {
      xpGained: 20,
      totalXp: 30,
      level: 1,
      leveledUp: false,
      currentLevelXp: 0,
      nextLevelXp: 100,
      levelProgress: 30,
      achievementsUnlocked: [],
      challengesCompleted: [],
    },
    ...overrides,
  }
}

describe('QuizzesPage', () => {
  it('lista os quizzes para visitantes, sem histórico', async () => {
    vi.mocked(getQuizzes).mockResolvedValue([
      listItem(),
      listItem({ id: 2, title: 'A Guerra dos Cem Anos', difficulty: 'Hard', category: 'História' }),
    ])
    renderRoute('/quizzes')

    const card = (await screen.findByRole('heading', { name: 'O Time Avatar' })).closest('article')!
    expect(within(card).getByText('Fácil')).toBeInTheDocument()
    expect(within(card).getByText('10 perguntas')).toBeInTheDocument()
    expect(within(card).getByRole('link', { name: 'Jogar: O Time Avatar' })).toHaveAttribute(
      'href',
      '/quizzes/1',
    )
    const hard = screen.getByRole('heading', { name: 'A Guerra dos Cem Anos' }).closest('article')!
    expect(within(hard).getByText('Difícil')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /Meu histórico/ })).not.toBeInTheDocument()
  })

  it('mostra o melhor resultado de quem está logado', async () => {
    mockSession()
    vi.mocked(getQuizzes).mockResolvedValue([
      listItem({ bestResult: { score: 8, totalQuestions: 10, percentage: 80, attempts: 2 } }),
    ])
    renderRoute('/quizzes')

    expect(await screen.findByText('Melhor: 8/10 (80%)')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Jogar de novo: O Time Avatar' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Meu histórico/ })).toHaveAttribute(
      'href',
      '/quizzes/history',
    )
  })

  it('filtra por dificuldade e guarda o filtro na URL', async () => {
    vi.mocked(getQuizzes).mockResolvedValue([listItem()])
    const { router } = renderRoute('/quizzes')
    await screen.findByRole('heading', { name: 'O Time Avatar' })

    await userEvent.setup().selectOptions(screen.getByLabelText('Dificuldade'), 'Hard')

    await waitFor(() => expect(getQuizzes).toHaveBeenCalledWith({ difficulty: 'Hard' }))
    expect(router.state.location.search).toBe('?difficulty=Hard')
  })

  it('sem resultado para o filtro, oferece limpar', async () => {
    vi.mocked(getQuizzes).mockImplementation(async (filters) =>
      filters?.difficulty ? [] : [listItem()],
    )
    renderRoute('/quizzes?difficulty=Hard')

    expect(await screen.findByText('Nenhum quiz encontrado')).toBeInTheDocument()
    await userEvent.setup().click(screen.getByRole('button', { name: 'Limpar filtros' }))
    expect(await screen.findByRole('heading', { name: 'O Time Avatar' })).toBeInTheDocument()
  })

  it('mostra erro com opção de tentar de novo', async () => {
    vi.mocked(getQuizzes).mockRejectedValue(new Error('Network Error'))
    renderRoute('/quizzes')

    expect(await screen.findByText('Não foi possível carregar os quizzes')).toBeInTheDocument()
  })
})

describe('QuizPlayPage', () => {
  async function answer(actions: ReturnType<typeof userEvent.setup>, option: string) {
    await actions.click(await screen.findByRole('radio', { name: option }))
  }

  it('uma pergunta por vez, com voltar, e envia as respostas para correção no backend', async () => {
    mockSession()
    vi.mocked(getQuiz).mockResolvedValue(quiz)
    vi.mocked(submitQuizAttempt).mockResolvedValue(attemptResult())
    const actions = userEvent.setup()
    renderRoute('/quizzes/1')

    expect(await screen.findByText('Pergunta 1 de 2')).toBeInTheDocument()
    const next = screen.getByRole('button', { name: 'Próxima' })
    expect(next).toBeDisabled()

    await answer(actions, 'Momo')
    await actions.click(next)
    expect(screen.getByRole('group', { name: 'Quem é o tio de Zuko?' })).toBeInTheDocument()

    // Volta e a resposta anterior continua marcada.
    await actions.click(screen.getByRole('button', { name: 'Anterior' }))
    expect(screen.getByRole('radio', { name: 'Momo' })).toBeChecked()
    await actions.click(screen.getByRole('button', { name: 'Próxima' }))

    const send = screen.getByRole('button', { name: 'Enviar respostas' })
    expect(send).toBeDisabled()
    await answer(actions, 'Iroh')
    await actions.click(send)

    expect(submitQuizAttempt).toHaveBeenCalledWith(1, [
      { questionId: 10, optionId: 101 },
      { questionId: 11, optionId: 111 },
    ])
    expect(await screen.findByRole('heading', { name: 'Bom começo!' })).toBeInTheDocument()
    expect(screen.getByText('1 / 2')).toBeInTheDocument()
    expect(screen.getByText('50% de acertos')).toBeInTheDocument()
    expect(screen.getByText(/Resposta correta:/)).toHaveTextContent('Resposta correta: Appa')
    expect(screen.getByText('Appa é o bisão; Momo é o lêmure.')).toBeInTheDocument()
    expect(await screen.findByText('🎯 Quiz concluído — 1/2! +20 XP')).toBeInTheDocument()
  })

  it('tentativa sem XP avisa que fica só no histórico; jogar de novo recomeça', async () => {
    mockSession()
    vi.mocked(getQuiz).mockResolvedValue(quiz)
    vi.mocked(submitQuizAttempt).mockResolvedValue(
      attemptResult({ experienceEarned: 0, reward: null }),
    )
    const actions = userEvent.setup()
    renderRoute('/quizzes/1')

    await answer(actions, 'Momo')
    await actions.click(screen.getByRole('button', { name: 'Próxima' }))
    await answer(actions, 'Iroh')
    await actions.click(screen.getByRole('button', { name: 'Enviar respostas' }))

    expect(await screen.findByText(/Você já recebeu o XP deste quiz/)).toBeInTheDocument()
    expect(screen.queryByText(/Quiz concluído/)).not.toBeInTheDocument()

    await actions.click(screen.getByRole('button', { name: 'Jogar novamente' }))
    expect(screen.getByText('Pergunta 1 de 2')).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: 'Momo' })).not.toBeChecked()
  })

  it('anuncia as conquistas desbloqueadas pelo quiz', async () => {
    mockSession()
    vi.mocked(getQuiz).mockResolvedValue(quiz)
    vi.mocked(submitQuizAttempt).mockResolvedValue(
      attemptResult({
        experienceEarned: 0,
        reward: {
          ...attemptResult().reward!,
          xpGained: 150,
          achievementsUnlocked: [
            { id: 5, name: 'Conhecedor de Avatar', icon: 'brain', experienceReward: 150 },
          ],
        },
      }),
    )
    const actions = userEvent.setup()
    renderRoute('/quizzes/1')

    await answer(actions, 'Appa')
    await actions.click(screen.getByRole('button', { name: 'Próxima' }))
    await answer(actions, 'Iroh')
    await actions.click(screen.getByRole('button', { name: 'Enviar respostas' }))

    expect(
      await screen.findByText('Conquista desbloqueada — Conhecedor de Avatar! +150 XP'),
    ).toBeInTheDocument()
    expect(
      within(screen.getByRole('list', { name: 'Conquistas desbloqueadas' })).getByText(
        /Conhecedor de Avatar/,
      ),
    ).toBeInTheDocument()
  })

  it('se o envio falhar, mantém as respostas e mostra o erro', async () => {
    mockSession()
    vi.mocked(getQuiz).mockResolvedValue(quiz)
    vi.mocked(submitQuizAttempt).mockRejectedValue(new Error('Network Error'))
    const actions = userEvent.setup()
    renderRoute('/quizzes/1')

    await answer(actions, 'Momo')
    await actions.click(screen.getByRole('button', { name: 'Próxima' }))
    await answer(actions, 'Iroh')
    await actions.click(screen.getByRole('button', { name: 'Enviar respostas' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Suas respostas foram mantidas')
    expect(screen.getByRole('radio', { name: 'Iroh' })).toBeChecked()
    expect(screen.getByRole('button', { name: 'Enviar respostas' })).toBeEnabled()
  })

  it('quiz inexistente mostra "não encontrado"', async () => {
    mockSession()
    vi.mocked(getQuiz).mockRejectedValue(apiError(404, "Quiz '999' não encontrado(a)."))
    renderRoute('/quizzes/999')

    expect(await screen.findByText('Quiz não encontrado')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Tentar novamente' })).not.toBeInTheDocument()
  })

  it('exige login para jogar', async () => {
    const { router } = renderRoute('/quizzes/1')
    await vi.waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(getQuiz).not.toHaveBeenCalled()
  })
})

describe('QuizHistoryPage', () => {
  it('lista as tentativas, inclusive as sem XP', async () => {
    mockSession()
    vi.mocked(getQuizHistory).mockResolvedValue(
      page([
        {
          id: 2,
          quizId: 1,
          quizTitle: 'O Time Avatar',
          score: 10,
          totalQuestions: 10,
          percentage: 100,
          experienceEarned: 0,
          completedAt: '2026-10-26T15:00:00Z',
        },
        {
          id: 1,
          quizId: 1,
          quizTitle: 'O Time Avatar',
          score: 7,
          totalQuestions: 10,
          percentage: 70,
          experienceEarned: 20,
          completedAt: '2026-10-26T14:00:00Z',
        },
      ]),
    )
    renderRoute('/quizzes/history')

    const items = await screen.findAllByRole('link', { name: 'O Time Avatar' })
    expect(items).toHaveLength(2)
    expect(items[0]).toHaveAttribute('href', '/quizzes/1')
    expect(screen.getByText('Sem XP')).toBeInTheDocument()
    expect(screen.getByText('+20 XP')).toBeInTheDocument()
    expect(getQuizHistory).toHaveBeenCalledWith(1)
  })

  it('sem tentativas, convida para jogar', async () => {
    mockSession()
    renderRoute('/quizzes/history')

    expect(await screen.findByText('Você ainda não jogou nenhum quiz')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Ver quizzes' })).toHaveAttribute('href', '/quizzes')
  })
})
