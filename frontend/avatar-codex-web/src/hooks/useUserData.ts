import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { getDiscoveries, getExperience, getFavorites, getStatistics } from '../services/userService'
import { useAuth } from './useAuth'

// Dados do usuário logado: a chave inclui o id, então outro login nunca vê o cache anterior.

function useUserKey() {
  const { user } = useAuth()
  return { userId: user?.id, enabled: Boolean(user) }
}

export function useFavorites(page: number) {
  const { userId, enabled } = useUserKey()
  return useQuery({
    queryKey: ['me', userId, 'favorites', page],
    queryFn: () => getFavorites(page),
    enabled,
    placeholderData: keepPreviousData,
  })
}

export function useRecentDiscoveries(pageSize = 6) {
  const { userId, enabled } = useUserKey()
  return useQuery({
    queryKey: ['me', userId, 'discoveries', pageSize],
    queryFn: () => getDiscoveries(pageSize),
    enabled,
  })
}

export function useExperienceHistory(page: number) {
  const { userId, enabled } = useUserKey()
  return useQuery({
    queryKey: ['me', userId, 'experience', page],
    queryFn: () => getExperience(page),
    enabled,
    placeholderData: keepPreviousData,
  })
}

export function useStatistics() {
  const { userId, enabled } = useUserKey()
  return useQuery({ queryKey: ['me', userId, 'statistics'], queryFn: getStatistics, enabled })
}

/** Depois de favoritar/descobrir: marcas dos personagens e dados do perfil ficam desatualizados. */
export function useInvalidateUserContent() {
  const queryClient = useQueryClient()
  return () => {
    void queryClient.invalidateQueries({ queryKey: ['characters'] })
    void queryClient.invalidateQueries({ queryKey: ['nations'] })
    void queryClient.invalidateQueries({ queryKey: ['me'] })
  }
}
