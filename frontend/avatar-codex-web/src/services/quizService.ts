import type { PagedResult } from '../types/content'
import type {
  Quiz,
  QuizAnswer,
  QuizAttempt,
  QuizAttemptResult,
  QuizFilters,
  QuizListItem,
} from '../types/quiz'
import { api } from './api'

export async function getQuizzes(filters: QuizFilters = {}): Promise<QuizListItem[]> {
  const { data } = await api.get<QuizListItem[]>('/api/quizzes', { params: filters })
  return data
}

export async function getQuiz(id: number): Promise<Quiz> {
  const { data } = await api.get<Quiz>(`/api/quizzes/${id}`)
  return data
}

export async function submitQuizAttempt(
  id: number,
  answers: QuizAnswer[],
): Promise<QuizAttemptResult> {
  const { data } = await api.post<QuizAttemptResult>(`/api/quizzes/${id}/attempts`, { answers })
  return data
}

export async function getQuizHistory(page: number): Promise<PagedResult<QuizAttempt>> {
  const { data } = await api.get<PagedResult<QuizAttempt>>('/api/users/me/quiz-history', {
    params: { page, pageSize: 10 },
  })
  return data
}
