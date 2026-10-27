import type { ElementSummary, Image } from './content'

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
