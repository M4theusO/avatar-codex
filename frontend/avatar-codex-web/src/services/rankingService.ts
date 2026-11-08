import type { Ranking, RankingQuery } from '../types/ranking'
import { api } from './api'

export const RANKING_PAGE_SIZE = 20

export async function getRanking(query: RankingQuery): Promise<Ranking> {
  const { data } = await api.get<Ranking>('/api/ranking', {
    params: { ...query, pageSize: RANKING_PAGE_SIZE },
  })
  return data
}
