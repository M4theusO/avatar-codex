import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getRanking } from '../services/rankingService'
import type { RankingQuery } from '../types/ranking'
import { useAuth } from './useAuth'

/** Sob a chave 'me': qualquer ganho de XP do usuário invalida o ranking junto com o resto do progresso. */
export function useRanking(query: RankingQuery) {
  const { user } = useAuth()
  return useQuery({
    queryKey: ['me', user?.id, 'ranking', query],
    queryFn: () => getRanking(query),
    enabled: Boolean(user),
    placeholderData: keepPreviousData,
  })
}
