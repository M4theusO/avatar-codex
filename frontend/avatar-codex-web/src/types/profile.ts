import type { ElementSummary } from './content'
import type { UserStatistics } from './progress'

/** Conquista desbloqueada, como aparece no perfil público. */
export interface ProfileAchievement {
  id: number
  name: string
  description: string
  icon: string
  experienceReward: number
  unlockedAt: string
}

/** GET /api/users/:username — só dados do jogo (nada de email ou id). */
export interface PublicProfile {
  username: string
  avatarUrl: string | null
  element: ElementSummary | null
  level: number
  experiencePoints: number
  currentLevelXp: number
  nextLevelXp: number
  levelProgress: number
  createdAt: string
  /** Quem consulta é o dono do perfil. */
  isCurrentUser: boolean
  isProfilePrivate: boolean
  followersCount: number
  followingCount: number
  /** Quem consulta segue este perfil (sempre false no próprio). */
  isFollowing: boolean
  statistics: UserStatistics
  achievements: ProfileAchievement[]
}

/** Pessoa nas listas de seguindo/seguidores; perfil privado aparece sem link. */
export interface FollowUser {
  username: string
  avatarUrl: string | null
  element: ElementSummary | null
  level: number
  isProfilePrivate: boolean
  followedAt: string
}
