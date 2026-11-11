import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { followUser, getFollowers, getFollowing, unfollowUser } from '../services/followService'
import type { PublicProfile } from '../types/profile'
import { getApiError } from '../utils/apiError'
import { useAuth } from './useAuth'
import { useToast } from './useToast'

export type FollowList = 'following' | 'followers'

/** Listas do próprio usuário, sob a chave 'me' (seguir/deixar de seguir as invalida). */
export function useFollowList(list: FollowList, page: number) {
  const { user } = useAuth()
  return useQuery({
    queryKey: ['me', user?.id, 'follows', list, page],
    queryFn: () => (list === 'following' ? getFollowing(page) : getFollowers(page)),
    enabled: Boolean(user),
    placeholderData: keepPreviousData,
  })
}

/**
 * Seguir/deixar de seguir a partir do perfil público, de forma otimista (como favoritar): o botão e o
 * número de seguidores mudam na hora e voltam se a API recusar. No fim, os dados do usuário são recarregados.
 */
export function useToggleFollow(username: string) {
  const { user } = useAuth()
  const { showToast } = useToast()
  const queryClient = useQueryClient()
  const profileKey = ['me', user?.id, 'profiles', username.toLowerCase()]

  return useMutation({
    mutationFn: (follow: boolean) => (follow ? followUser(username) : unfollowUser(username)),
    onMutate: async (follow) => {
      await queryClient.cancelQueries({ queryKey: profileKey })
      const previous = queryClient.getQueryData<PublicProfile>(profileKey)
      if (previous) {
        queryClient.setQueryData<PublicProfile>(profileKey, {
          ...previous,
          isFollowing: follow,
          followersCount: Math.max(0, previous.followersCount + (follow ? 1 : -1)),
        })
      }
      return { previous }
    },
    onError: (error, _follow, context) => {
      if (context?.previous) queryClient.setQueryData(profileKey, context.previous)
      showToast(getApiError(error).message, 'error')
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ['me'] }),
  })
}
