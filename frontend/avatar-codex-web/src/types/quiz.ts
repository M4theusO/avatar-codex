import type { Image } from './content'
import type { Reward } from './progress'

// Espelham os DTOs de quiz da API (docs/api.md §6 Quiz).

export type QuizDifficulty = 'Easy' | 'Medium' | 'Hard'

export interface QuizFilters {
  category?: string
  difficulty?: QuizDifficulty
}

export interface QuizBestResult {
  score: number
  totalQuestions: number
  percentage: number
  attempts: number
}

export interface QuizListItem {
  id: number
  title: string
  description: string
  category: string
  difficulty: QuizDifficulty
  image: Image | null
  questionCount: number
  /** Só para quem está logado e já jogou. */
  bestResult: QuizBestResult | null
}

/** Opções para jogar: a API não envia qual é a correta (RN-09). */
export interface QuizOption {
  id: number
  text: string
}

export interface QuizQuestion {
  id: number
  text: string
  imageUrl: string | null
  options: QuizOption[]
}

export interface Quiz {
  id: number
  title: string
  description: string
  category: string
  difficulty: QuizDifficulty
  image: Image | null
  questions: QuizQuestion[]
}

export interface QuizAnswer {
  questionId: number
  optionId: number
}

export interface QuestionResult {
  questionId: number
  selectedOptionId: number
  correctOptionId: number
  isCorrect: boolean
  explanation: string | null
}

export interface QuizAttemptResult {
  attemptId: number
  score: number
  totalQuestions: number
  percentage: number
  /** XP do quiz nesta tentativa (0 quando ele já tinha sido concluído/gabaritado). */
  experienceEarned: number
  results: QuestionResult[]
  reward: Reward | null
}

export interface QuizAttempt {
  id: number
  quizId: number
  quizTitle: string
  score: number
  totalQuestions: number
  percentage: number
  experienceEarned: number
  completedAt: string
}
