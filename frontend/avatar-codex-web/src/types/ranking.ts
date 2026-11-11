import type { ElementSummary, PagedResult } from './content'

export type RankingPeriod = 'all' | 'month'

/** Parâmetros de GET /api/ranking. */
export interface RankingQuery {
  period: RankingPeriod
  elementId?: number
  /** Só você e quem você segue (aba "Seguindo"). */
  following?: boolean
  page: number
}

/** Só dados públicos do jogo; `experience` é o XP do período. */
export interface RankingEntry {
  position: number
  username: string
  /** Foto de perfil (relativa à API), ou null. */
  avatarUrl: string | null
  level: number
  experience: number
  element: ElementSummary | null
  isCurrentUser: boolean
  /** Perfil privado: o nome não leva ao perfil público. */
  isProfilePrivate: boolean
}

export interface Ranking {
  period: RankingPeriod
  /** Mês do ranking mensal (UTC); null no geral. */
  periodStart: string | null
  periodEnd: string | null
  elementId: number | null
  following: boolean
  entries: PagedResult<RankingEntry>
  /** Posição de quem consulta, ou null quando não entra neste ranking. */
  currentUser: { position: number; experience: number } | null
}
