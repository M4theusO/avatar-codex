import { useQuery } from '@tanstack/react-query'
import { getPublicProfile } from '../services/profileService'
import { useAuth } from './useAuth'

/** Sob a chave 'me': o próprio progresso (ou a troca de visibilidade) invalida o perfil junto com o resto. */
export function usePublicProfile(username: string) {
  const { user } = useAuth()
  return useQuery({
    queryKey: ['me', user?.id, 'profiles', username.toLowerCase()],
    queryFn: () => getPublicProfile(username),
    enabled: Boolean(user) && username.length > 0,
  })
}
