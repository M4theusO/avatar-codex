import type { ElementSummary } from './content'
import type { Reward } from './progress'

// Espelham os DTOs de autenticação da API (ver docs/api.md §6 Auth).

export interface User {
  id: string
  username: string
  email: string
  avatarUrl: string | null
  element: ElementSummary | null
  level: number
  experiencePoints: number
  /** XP mínimo do nível atual e do próximo, e a porcentagem da barra: tudo calculado no backend. */
  currentLevelXp: number
  nextLevelXp: number
  levelProgress: number
  createdAt: string
}

export interface AuthResponse {
  accessToken: string
  expiresIn: number
  user: User
  /** Só no cadastro: a conquista "Primeiro passo". */
  reward: Reward | null
}

/** Resultado de login/cadastro: o usuário e, no cadastro, o `reward` do "Primeiro passo". */
export interface SessionResult {
  user: User
  reward: Reward | null
}

export interface LoginData {
  email: string
  password: string
}

export interface RegisterData {
  username: string
  email: string
  password: string
  confirmPassword: string
}
