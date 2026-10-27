import { keepPreviousData, useMutation, useQuery } from '@tanstack/react-query'
import { getQuiz, getQuizHistory, getQuizzes, submitQuizAttempt } from '../services/quizService'
import type { QuizAnswer, QuizFilters } from '../types/quiz'
import { useAuth } from './useAuth'
import { useInvalidateUserContent } from './useUserData'

/** O catálogo traz o melhor resultado de quem está logado: cache separado por usuário. */
export function useQuizzes(filters: QuizFilters = {}) {
  const { status, user } = useAuth()
  return useQuery({
    queryKey: ['quizzes', user?.id ?? 'anonymous', filters],
    queryFn: () => getQuizzes(filters),
    enabled: status !== 'loading',
    placeholderData: keepPreviousData,
  })
}

export function useQuiz(id: number) {
  return useQuery({
    queryKey: ['quiz', id],
    queryFn: () => getQuiz(id),
    enabled: Number.isInteger(id) && id > 0,
    // As perguntas não mudam durante o jogo: nada de refetch ao voltar para a aba.
    staleTime: Infinity,
  })
}

export function useSubmitQuiz(id: number) {
  const invalidate = useInvalidateUserContent()
  return useMutation({
    mutationFn: (answers: QuizAnswer[]) => submitQuizAttempt(id, answers),
    // Melhor resultado, histórico, estatísticas, XP e conquistas mudam depois de uma tentativa.
    onSuccess: invalidate,
  })
}

export function useQuizHistory(page: number) {
  const { user } = useAuth()
  return useQuery({
    queryKey: ['me', user?.id, 'quiz-history', page],
    queryFn: () => getQuizHistory(page),
    enabled: Boolean(user),
    placeholderData: keepPreviousData,
  })
}
