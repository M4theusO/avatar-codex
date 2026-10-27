import type { User } from './auth'
import type { CharacterListItem, ElementSummary, Image } from './content'
import type { QuizListItem } from './quiz'

// Espelham os DTOs de progresso da API (docs/api.md §5 e §6).

export interface AchievementReward {
  id: number
  name: string
  icon: string
  experienceReward: number
}

/** `xpGained` soma o XP do evento e o das conquistas desbloqueadas por ele. */
export interface Reward {
  xpGained: number
  totalXp: number
  level: number
  leveledUp: boolean
  currentLevelXp: number
  nextLevelXp: number
  levelProgress: number
  achievementsUnlocked: AchievementReward[]
  challengesCompleted: { id: number; title: string; experienceReward: number }[]
}

export interface DiscoveryResult {
  isNew: boolean
  reward: Reward | null
}

export type DiscoveryKind = 'Character' | 'Nation'

export interface Discovery {
  kind: DiscoveryKind
  id: number
  name: string
  image: Image | null
  element: ElementSummary | null
  discoveredAt: string
}

export type ExperienceType =
  | 'CharacterDiscovered'
  | 'NationDiscovered'
  | 'QuizCompleted'
  | 'QuizPerfect'
  | 'ChallengeCompleted'
  | 'AchievementUnlocked'

export interface ExperienceTransaction {
  id: number
  amount: number
  type: ExperienceType
  description: string
  createdAt: string
}

export interface UserStatistics {
  discoveredCharacters: number
  totalCharacters: number
  discoveredNations: number
  totalNations: number
  favorites: number
  unlockedAchievements: number
  totalAchievements: number
  completedQuizzes: number
  totalQuizzes: number
  quizAttempts: number
  /** Acertos / respostas em todas as tentativas (0–100); null sem tentativas. */
  accuracyPercent: number | null
}

/** Conquista do ponto de vista do usuário: `progress`/`target` vêm prontos do backend. */
export interface UserAchievement {
  id: number
  name: string
  description: string
  icon: string
  experienceReward: number
  isUnlocked: boolean
  unlockedAt: string | null
  progress: number
  target: number
}

/** Página inicial do usuário (GET /api/users/me/dashboard): as escolhas vêm prontas do backend. */
export interface Dashboard {
  user: User
  statistics: UserStatistics
  /** Desbloqueadas, mais recentes primeiro. */
  recentAchievements: UserAchievement[]
  /** A bloqueada mais perto da meta; null quando todas foram desbloqueadas. */
  nextAchievement: UserAchievement | null
  recentDiscoveries: Discovery[]
  /** Personagens ainda não descobertos (mudam uma vez por dia). */
  recommendations: CharacterListItem[]
  /** null quando todos os quizzes foram gabaritados. */
  featuredQuiz: QuizListItem | null
  favorites: CharacterListItem[]
}
