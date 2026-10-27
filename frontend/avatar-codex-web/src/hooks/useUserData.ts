import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import {
  getAchievements,
  getDashboard,
  getDiscoveries,
  getExperience,
  getFavorites,
  getStatistics,
} from '../services/userService'
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

export function useAchievements() {
  const { userId, enabled } = useUserKey()
  return useQuery({ queryKey: ['me', userId, 'achievements'], queryFn: getAchievements, enabled })
}

/**
 * Dashboard numa chamada. O XP/nível que vem nele atualiza o usuário em memória (navbar e saudação),
 * caso tenha mudado em outra aba ou dispositivo.
 */
export function useDashboard() {
  const { userId, enabled } = useUserKey()
  const { updateUser } = useAuth()
  const query = useQuery({ queryKey: ['me', userId, 'dashboard'], queryFn: getDashboard, enabled })
  const freshUser = query.data?.user

  useEffect(() => {
    if (!freshUser) return
    updateUser({
      experiencePoints: freshUser.experiencePoints,
      level: freshUser.level,
      currentLevelXp: freshUser.currentLevelXp,
      nextLevelXp: freshUser.nextLevelXp,
      levelProgress: freshUser.levelProgress,
    })
  }, [freshUser, updateUser])

  return query
}

/**
 * Depois de favoritar, descobrir ou jogar: marcas dos personagens, melhor resultado dos quizzes e
 * dados do perfil ficam desatualizados.
 */
export function useInvalidateUserContent() {
  const queryClient = useQueryClient()
  return () => {
    void queryClient.invalidateQueries({ queryKey: ['characters'] })
    void queryClient.invalidateQueries({ queryKey: ['nations'] })
    void queryClient.invalidateQueries({ queryKey: ['quizzes'] })
    void queryClient.invalidateQueries({ queryKey: ['me'] })
  }
}
